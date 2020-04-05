using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streams;
using Shipbot.Controller.Core.Apps.GrainState;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Apps.Streaming;
using Shipbot.Controller.Core.Configuration.ApplicationSources;
using Shipbot.Controller.Core.Configuration.Apps;
using Shipbot.Controller.Core.ContainerRegistry.Models;
using Shipbot.Controller.Core.Models;
using Shipbot.Controller.Core.Utilities;

namespace Shipbot.Controller.Core.Apps.Grains
{
    public interface IApplicationEnvironmentGrain : IGrainWithStringKey
    {
        Task<IEnumerable<Image>> GetImages();
        Task EnableAutoDeploy();
        Task DisableAutoDeploy();
        Task SetImageTag(Image image, string newImageTag);
        Task<IReadOnlyDictionary<Image, string>> GetCurrentImageTags();
        Task Configure(ApplicationEnvironmentSettings applicationEnvironmentSettings);
        Task<IEnumerable<string>> GetDeploymentPromotionSettings();
        Task<ImageUpdatePolicy> GetImageUpdatePolicy(Image image);
        Task<bool> IsAutoDeployEnabled();
        Task CheckForMissedImageTags();
        Task StartListeningToImageTagUpdates();
        Task StopListeningToImageTagUpdates();
    }
    
    [StorageProvider(ProviderName = ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)]
    public class ApplicationEnvironmentGrain : Grain<ApplicationEnvironment>, IApplicationEnvironmentGrain
    {
        private readonly Dictionary<Guid, StreamSubscriptionHandle<ImageTag>> _streamSubscriptionHandles = new Dictionary<Guid, StreamSubscriptionHandle<ImageTag>>();

        private readonly ILogger<ApplicationEnvironmentGrain> _log;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;
        private ApplicationEnvironmentKey _key;
        private IStreamProvider _streamProvider;

        public ApplicationEnvironmentGrain(
            ILogger<ApplicationEnvironmentGrain> log,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory
            )
        {
            _log = log;
            _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory;
        }
        
        public override async Task OnActivateAsync()
        {
            if (State.Images == null) State.Images = new List<Image>();
            if (State.PromotionEnvironments == null) State.PromotionEnvironments = new List<string>();
            //if (State.CurrentImageTags == null) State.CurrentImageTags = new List<(Image, string)>();

            var key = this.GetPrimaryKeyString();
            _key = ApplicationEnvironmentKey.Parse(key);

            _streamProvider = GetStreamProvider(Constants.InternalMessageStreamProvider);
            
            await StartListeningToImageTagUpdates();

            await base.OnActivateAsync();
        }

        public async Task StartListeningToImageTagUpdates()
        {
            // register for image updates
            foreach (var image in State.Images)
            {
                await StartListeningToImageTagUpdates(image);
            }
        }
        
        private async Task<StreamSubscriptionHandle<ImageTag>> GetStreamSubscriptionHandle(
            IAsyncStream<ImageTag> asyncStream, 
            Image image
        )
        {
            if (asyncStream == null) throw new ArgumentNullException(nameof(asyncStream));
                
            var handles = await asyncStream.GetAllSubscriptionHandles();
                
            var imageUpdateSettings = State.ImageUpdateSettings
                .First(x => Image.EqualityComparer.Equals(x.Image, image));
                    
            var logger = _loggerFactory.CreateLogger<ContainerRegistryStreamObserver>();
            var grainFactory = _serviceProvider.GetService<IGrainFactory>();
                    
            var streamObserver = new ContainerRegistryStreamObserver(
                logger,
                image,
                imageUpdateSettings,
                _key,
                grainFactory
            );

            return handles.Any()
                ? await handles.First().ResumeAsync(streamObserver)
                : await asyncStream.SubscribeAsync(
                    streamObserver,
                    null,
                    StreamFilterFunc);
        }


        private async Task StartListeningToImageTagUpdates(Image image)
        {

            _log.LogInformation("Listening to image update events for {image}", image);
            
            var streamId = image.Repository.CreateGuidFromString();
            
            var stream = _streamProvider.GetStream<ImageTag>(streamId,
                Constants.ContainerRegistryQueueNamespace);
            var streamHandle = await GetStreamSubscriptionHandle(stream, image);

            _streamSubscriptionHandles[streamId] = streamHandle;
        }

        public async Task StopListeningToImageTagUpdates()
        {
            foreach (var streamSubscriptionHandle in _streamSubscriptionHandles)
            {
                await streamSubscriptionHandle.Value.UnsubscribeAsync();
            }
            
            _streamSubscriptionHandles.Clear();
        }

        public static bool StreamFilterFunc(IStreamIdentity stream, object filterdata, object item)
        {
            return true;
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
            using (_log.BeginScope(new Dictionary<string, object>()
            {
                {"Application", _key.Application},
                {"Environment", _key.Environment}
            }))
            {

                _log.LogInformation("Starting application environment configuration for '{application}:{environment}'",
                    _key.Application, _key.Environment);

                // clear
                await StopListeningToImageTagUpdates();
                State.Images.Clear();
                State.PromotionEnvironments.Clear();
                State.CurrentImageTags.Clear();

                // update state
                foreach (var imageSettings in applicationEnvironmentSettings.Images)
                {
                    var image = new Image(
                        imageSettings.Repository,
                        new TagProperty(
                            imageSettings.TagProperty.Path,
                            imageSettings.TagProperty.ValueFormat
                        )
                    );

                    var imageUpdateSettings = new ImageUpdateSettings()
                    {
                        Image = image,
                        Policy = imageSettings.Policy switch
                            {
                                UpdatePolicy.Glob => (ImageUpdatePolicy) new GlobImageUpdatePolicy(
                                    imageSettings.Pattern),
                                UpdatePolicy.Regex => new RegexImageUpdatePolicy(imageSettings.Pattern),
                                _ => throw new NotImplementedException()
                            }
                    };
                    
                    State.Images.Add(image);
                    State.ImageUpdateSettings.Add(imageUpdateSettings);
                }

                State.ApplicationSourceSettings = applicationEnvironmentSettings.Source;
                State.AutoDeploy = applicationEnvironmentSettings.AutoDeploy;

                State.PromotionEnvironments.AddRange(applicationEnvironmentSettings.PromoteTargets);
                State.Images.ForEach(image => { State.CurrentImageTags.Add(new DeployedImageTag() {Image = image}); });

                await WriteStateAsync();
                
                // fetch current deployment sources, current image tags and apply them
                var deploymentSourceGrain = State.ApplicationSourceSettings.Type switch
                {
                    ApplicationSourceType.Helm => GrainFactory.GetHelmDeploymentSourceGrain(_key),
                    _ => throw new InvalidOperationException()
                };
                await deploymentSourceGrain.Configure(
                    State.ApplicationSourceSettings,
                    _key
                );
                
                await deploymentSourceGrain.Checkout();
                await deploymentSourceGrain.Refresh();
                var currentTags = await deploymentSourceGrain.GetImageTags();
                foreach (var keyValuePair in currentTags)
                {
                    await SetImageTag(keyValuePair.Key, keyValuePair.Value);
                }

                // activate the helm deployment repo watcher and updater.
                await deploymentSourceGrain.Activate();
            }
        }

        public async Task CheckForMissedImageTags()
        {
            foreach (var imageUpdateSetting in State.ImageUpdateSettings)
            {
                var image = imageUpdateSetting.Image;
                // await StartListeningToImageTagUpdates(image);
                
                var repositoryWatcherGrain = GrainFactory.GetElasticContainerRegistryWatcher(image.Repository);
                var repositoryTags = await repositoryWatcherGrain.GetTags();
                var latestTag = repositoryTags
                    .Where(t => imageUpdateSetting.Policy.IsMatch(t.Tag))
                    .OrderBy(x => x.CreatedAt)
                    .LastOrDefault();
                
                if (latestTag != null)
                {
                    if (State.CurrentImageTags.TryGetValue(image, out var currentTag))
                    {
                        if ((!imageUpdateSetting.Policy.IsMatch(currentTag.Tag)) ||
                            imageUpdateSetting.Policy.IsGreaterThen(latestTag.Tag, currentTag.Tag))
                        {
                            _log.Info(
                                "Found newer image for {image} ({currentTag} -> {nextTag})",
                                image.Repository,
                                currentTag.Tag,
                                latestTag.Tag
                            );
                            var streamId = image.Repository.CreateGuidFromString();
                            var stream = _streamProvider.GetStream<ImageTag>(streamId,
                                Constants.ContainerRegistryQueueNamespace);
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

        public Task<ImageUpdatePolicy> GetImageUpdatePolicy(Image image)
        {
            return Task.FromResult(
                State.ImageUpdateSettings
                    .FirstOrDefault(x => Image.EqualityComparer.Equals(x.Image, image))?.Policy
                );
        }

        public Task SetImageTag(Image image, string newImageTag)
        {
            _log.LogTrace("ApplicationEnvironmentGrain::SetImageTag({Repository}, {Tag}) >>", 
                image.Repository, newImageTag);

            if (!State.CurrentImageTags.TryGetValue(image, out var deployedImageTag)) 
                return Task.CompletedTask;
            
            if (deployedImageTag.Tag == newImageTag)
                return Task.CompletedTask;
            
            _log.LogInformation(
                "{Application}-{Environment}: Setting tag for '{Repository}' to '{Tag}' from {CurrentTag}",
                _key.Application,
                _key.Environment,
                image.Repository,
                newImageTag,
                deployedImageTag.Tag
            );
            deployedImageTag.Tag = newImageTag;

            return WriteStateAsync();
        }

        public Task<IReadOnlyDictionary<Image, string>> GetCurrentImageTags()
        {
            return Task.FromResult(
                (IReadOnlyDictionary<Image, string>) State.CurrentImageTags
                    .ToDictionary(x => x.Image, x => x.Tag, Image.EqualityComparer
                    )
            );
        }

        public Task<IEnumerable<Image>> GetImages()
        {
            return Task.FromResult(State.Images.ToList().AsEnumerable());
        }
    }
}