using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streams;
using Shipbot.Controller.Core.Apps.GrainState;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Configuration.ApplicationSources;
using Shipbot.Controller.Core.Configuration.Apps;
using Shipbot.Controller.Core.ContainerRegistry;
using Shipbot.Controller.Core.ContainerRegistry.Models;
using Shipbot.Controller.Core.Deployments.Events;
using Shipbot.Controller.Core.Utilities;
using Shipbot.Controller.Core.Utilities.Eventing;

namespace Shipbot.Controller.Core.Apps.Grains
{
    public interface IApplicationEnvironmentGrain : IGrainWithStringKey
    {
        Task<IEnumerable<ApplicationEnvironmentImageMetadata>> GetImages();
        Task EnableAutoDeploy();
        Task DisableAutoDeploy();
        
        Task<string> GetImageTag(string imageTagValuePath);
        Task<string> GetImageTag(ApplicationEnvironmentImageKey image);
        
        Task SetImageTag(string imageTagValuePath, string newImageTag);
        Task SetImageTag(ApplicationEnvironmentImageMetadata image, string newImageTag);
        Task<IReadOnlyDictionary<ApplicationEnvironmentImageMetadata, string>> GetCurrentImageTags();
        Task Configure(ApplicationEnvironmentSettings applicationEnvironmentSettings);
        Task<IEnumerable<string>> GetDeploymentPromotionSettings();
        Task<bool> IsAutoDeployEnabled();
        Task CheckForMissedImageTags();
        Task StartListeningToImageTagUpdates();
        Task StopListeningToImageTagUpdates();
    }
    
    [StorageProvider(ProviderName = ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)]
    public class ApplicationEnvironmentGrain : EventHandlingGrain<ApplicationEnvironment>, IApplicationEnvironmentGrain
    {
        private readonly Dictionary<Guid, StreamSubscriptionHandle<ImageTag>> _streamSubscriptionHandles = new Dictionary<Guid, StreamSubscriptionHandle<ImageTag>>();

        private readonly ILogger<ApplicationEnvironmentGrain> _log;
        private IStreamProvider? _containerRegistryStreamProvider;
        private IStreamProvider? _eventHandlingStreamProvider;

        public ApplicationEnvironmentGrain(
            ILogger<ApplicationEnvironmentGrain> log
        )
        {
            _log = log;
        }
        
        public override async Task OnActivateAsync()
        {
            var key = ApplicationEnvironmentKey.Parse(this.GetPrimaryKeyString());
            
            State.PromotionEnvironments ??= new List<string>();

            using var _ = _log.BeginShipbotLogScope(key);

            // register our-self with application 
            var applicationGrain = GrainFactory.GetApplication(key.Application);
            await applicationGrain.RegisterEnvironment(key);
            
            _containerRegistryStreamProvider = GetStreamProvider(ContainerRegistryStreamingConstants.ContainerRegistryStreamProvider);
            _eventHandlingStreamProvider = GetStreamProvider(EventingStreamingConstants.EventHandlingStreamProvider);
            
            await StartListeningToImageTagUpdates();

            await base.OnActivateAsync();
        }

        public async Task StartListeningToImageTagUpdates()
        {
            using var _ = _log.BeginShipbotLogScope(ApplicationEnvironmentKey.Parse(this.GetPrimaryKeyString()));
            
            // register for image updates
            foreach (var image in State.Images)
            {
                await StartListeningToImageTagUpdates(image);
            }
        }

        private async Task StartListeningToImageTagUpdates(ApplicationEnvironmentImageMetadata image)
        {
            using var _ = _log.BeginShipbotLogScope(ApplicationEnvironmentKey.Parse(this.GetPrimaryKeyString()));

            _log.LogInformation("Listening to image update events for {image}", image);
            
            var streamId = image.Repository.CreateGuidFromString();
            
            var stream = _containerRegistryStreamProvider.GetStream<ImageTag>(streamId,
                ContainerRegistryStreamingConstants.ContainerRegistryQueueNamespace);
            var streamHandle = await GetStreamSubscriptionHandle(stream, image);

            _streamSubscriptionHandles[streamId] = streamHandle;
        }
        
        private async Task<StreamSubscriptionHandle<ImageTag>> GetStreamSubscriptionHandle(
            IAsyncStream<ImageTag> asyncStream, 
            ApplicationEnvironmentImageMetadata image
        )
        {
            if (asyncStream == null) throw new ArgumentNullException(nameof(asyncStream));
                
            var handles = await asyncStream.GetAllSubscriptionHandles();

            return handles.Any()
                ? await handles.First().ResumeAsync(OnNewImageTag)
                : await asyncStream.SubscribeAsync(OnNewImageTag);
        }

        private async Task OnNewImageTag(ImageTag item, StreamSequenceToken arg2)
        {
            var key = ApplicationEnvironmentKey.Parse(this.GetPrimaryKeyString());
            
            using var _ = _log.BeginShipbotLogScope(key);
            
            var image = State.Images.First(x => (item.Repository == x.Repository));

            if (image.Policy.IsMatch(item.Tag))
            {
                _log.Info("Received new image notification for {image}, with tag {tag}",
                    image,
                    item.Tag);

                var currentTag = image.CurrentTag;

                // handle the case where the current image tag does not match the policy specified (for example, manually updated or overridden)
                // TODO: maybe we don't want to do this and we may need to put a flag or a facility to force
                if ((!image.Policy.IsMatch(currentTag)) ||
                    image.Policy.IsGreaterThen(item.Tag, currentTag))
                {
                    var e = new NewDeploymentEvent(
                        key,
                        image,
                        currentTag,
                        item.Tag,
                        State.PromotionEnvironments.Any(),
                        State.PromotionEnvironments,
                        !State.AutoDeploy 
                    );
                    await SendEvent(e);
                }
            }
        }

        public async Task StopListeningToImageTagUpdates()
        {
            using (_log.BeginShipbotLogScope(ApplicationEnvironmentKey.Parse(this.GetPrimaryKeyString())))
            {
                foreach (var streamSubscriptionHandle in _streamSubscriptionHandles)
                {
                    await streamSubscriptionHandle.Value.UnsubscribeAsync();
                }

                _streamSubscriptionHandles.Clear();
            }
        }
        
        public Task<bool> IsAutoDeployEnabled()
        {
            return Task.FromResult(State.AutoDeploy);
        }

        public Task EnableAutoDeploy()
        {
            State.AutoDeploy = true;
            return Task.CompletedTask;
        }
        
        public Task DisableAutoDeploy()
        {
            State.AutoDeploy = false;
            return Task.CompletedTask;
        }

        public async Task Configure(ApplicationEnvironmentSettings applicationEnvironmentSettings)
        {
            var key = ApplicationEnvironmentKey.Parse(this.GetPrimaryKeyString());
            using var _ = _log.BeginShipbotLogScope(key);

            _log.LogInformation(
                "Starting application environment configuration"
            );

            // clear
            await StopListeningToImageTagUpdates();
            State.PromotionEnvironments.Clear();

            // update state
            foreach (var imageSettings in applicationEnvironmentSettings.Images)
            {
                var image = new ApplicationEnvironmentImageMetadata(
                    imageSettings.Repository,
                    string.Empty,
                    imageSettings.TagProperty.Path,
                    imageSettings.TagProperty.ValueFormat,
                    imageSettings.Policy switch
                    {
                        UpdatePolicy.Glob => (ImageUpdatePolicy) new GlobImageUpdatePolicy(
                            imageSettings.Pattern),
                        UpdatePolicy.Regex => new RegexImageUpdatePolicy(imageSettings.Pattern),
                        _ => throw new NotImplementedException()
                    }
                );

                State.Images.Add(image);
            }

            State.ApplicationSourceSettings = applicationEnvironmentSettings.Source;
            State.AutoDeploy = applicationEnvironmentSettings.AutoDeploy;

            State.PromotionEnvironments.AddRange(applicationEnvironmentSettings.PromoteTargets);

            await WriteStateAsync();
        }

        public async Task CheckForMissedImageTags()
        {
            foreach (var imageUpdateSetting in State.Images)
            {
                var image = imageUpdateSetting;
                // await StartListeningToImageTagUpdates(image);
                
                var repositoryWatcherGrain = GrainFactory.GetElasticContainerRegistryWatcher(image.Repository);
                var repositoryTags = await repositoryWatcherGrain.GetTags();
                var latestTag = repositoryTags
                    .Where(t => imageUpdateSetting.Policy.IsMatch(t.Tag))
                    .OrderBy(x => x.CreatedAt)
                    .LastOrDefault();
                
                if (latestTag != null)
                {
                    if (State.Images.TryGetValue(image, out var currentTag))
                    {
                        if ((!imageUpdateSetting.Policy.IsMatch(currentTag.CurrentTag)) ||
                            imageUpdateSetting.Policy.IsGreaterThen(latestTag.Tag, currentTag.CurrentTag))
                        {
                            _log.Info(
                                "Found newer image for {image} ({currentTag} -> {nextTag})",
                                image.Repository,
                                currentTag.CurrentTag,
                                latestTag.Tag
                            );
                            var streamId = image.Repository.CreateGuidFromString();
                            var stream = _containerRegistryStreamProvider.GetStream<ImageTag>(streamId,
                                ContainerRegistryStreamingConstants.ContainerRegistryQueueNamespace);
                            await stream.OnNextAsync(latestTag);
                        }
                    }
                }
                
                await repositoryWatcherGrain.Start();
            }
        }

        public Task<IEnumerable<string>> GetDeploymentPromotionSettings()
        {
            return Task.FromResult(State.PromotionEnvironments.ToArray().AsEnumerable());
        }

        public Task<string> GetImageTag(ApplicationEnvironmentImageKey image)
        {
            foreach (var imageMetadata in State.Images)
            {
                if (imageMetadata.Repository == image.Repository &&
                    imageMetadata.ImageTagValuePath == image.ImageTagValuePath)
                {
                    return Task.FromResult(imageMetadata.CurrentTag);    
                }
            }

            throw new InvalidOperationException();
        }

        public Task SetImageTag(string imageTagValuePath, string newImageTag)
        {
            var imageSettings = State.Images.First(
                x => x.ImageTagValuePath == imageTagValuePath
            );
            return SetImageTag(imageSettings, newImageTag);
        }

        public Task<string> GetImageTag(string imageTagValuePath)
        {
            var imageSettings = State.Images.First(
                x => x.ImageTagValuePath == imageTagValuePath
            );
            return Task.FromResult(imageSettings.CurrentTag);
        }

        public Task SetImageTag(ApplicationEnvironmentImageMetadata image, string newImageTag)
        {
            var key = ApplicationEnvironmentKey.Parse(this.GetPrimaryKeyString());
            using var _ = _log.BeginShipbotLogScope(key);;

            if (!State.Images.TryGetValue(image, out var deployedImageTag)) 
                return Task.CompletedTask;
            
            if (deployedImageTag.CurrentTag == newImageTag)
                return Task.CompletedTask;
            
            _log.LogInformation(
                "Setting tag for '{Repository}' to '{Tag}' from {CurrentTag}",
                key.Application,
                key.Environment,
                image.Repository,
                newImageTag,
                deployedImageTag.CurrentTag
            );
            deployedImageTag.CurrentTag = newImageTag;

            return WriteStateAsync();
        }

        public Task<IReadOnlyDictionary<ApplicationEnvironmentImageMetadata, string>> GetCurrentImageTags()
        {
            return Task.FromResult(
                (IReadOnlyDictionary<ApplicationEnvironmentImageMetadata, string>) State.Images
                    .ToDictionary(
                        x => x, 
                        x => x.CurrentTag, 
                        ApplicationEnvironmentImageMetadata.EqualityComparer
                    )
            );
        }

        public Task<IEnumerable<ApplicationEnvironmentImageMetadata>> GetImages()
        {
            return Task.FromResult(State.Images.ToList().AsEnumerable());
        }
    }
}