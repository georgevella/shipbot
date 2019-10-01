using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using Shipbot.Controller.Core.ApplicationSources;
using Shipbot.Controller.Core.Apps;
using Shipbot.Controller.Core.Models;
using Shipbot.Controller.Core.Registry.Watcher;
using Shipbot.Controller.Core.Slack;

namespace Shipbot.Controller.Core.Deployments
{
    public class DeploymentService : IDeploymentService
    {
        class PendingDeploymentKey
        {
            protected bool Equals(PendingDeploymentKey other)
            {
                return Equals(Application, other.Application) && Equals(ApplicationEnvironment, other.ApplicationEnvironment);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((PendingDeploymentKey) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Application != null ? Application.GetHashCode() : 0) * 397) ^ (ApplicationEnvironment != null ? ApplicationEnvironment.GetHashCode() : 0);
                }
            }

            public Application Application { get; }
            public ApplicationEnvironment ApplicationEnvironment { get; }

            public PendingDeploymentKey(Application application, ApplicationEnvironment applicationEnvironment)
            {
                Application = application;
                ApplicationEnvironment = applicationEnvironment;
            }
        }
        
        private readonly ConcurrentDictionary<DeploymentUpdate, DeploymentUpdateStatus> _deploymentUpdates = new ConcurrentDictionary<DeploymentUpdate, DeploymentUpdateStatus>();
        
        private readonly ConcurrentDictionary<PendingDeploymentKey, ConcurrentQueue<DeploymentUpdate>> _pendingDeploymentUpdates = new ConcurrentDictionary<PendingDeploymentKey, ConcurrentQueue<DeploymentUpdate>>();
        
        private readonly ConcurrentDictionary<DeploymentUpdate, IMessageHandle> _notificationHandles = new ConcurrentDictionary<DeploymentUpdate, IMessageHandle>();
        
        private readonly IApplicationService _applicationService;

        private readonly ILogger _log;
        
        private readonly ISlackClient _slackClient;

        public DeploymentService(
            ILogger<DeploymentService> log,
            IApplicationService applicationService,
            ISlackClient slackClient
        )
        {
            _log = log;
            _applicationService = applicationService;
            _slackClient = slackClient;
        }

        public async Task AddDeploymentUpdate(string containerRepository, IEnumerable<ImageTag> tags)
        {
            if (containerRepository == null) throw new ArgumentNullException(nameof(containerRepository));
            if (tags == null) throw new ArgumentNullException(nameof(tags));

            var tagCollection = tags.ToList();
            
            
            foreach (var application in _applicationService.GetApplications())
            {
                foreach (var env in application.Environments)
                {
                    if (!env.Value.AutoDeploy) 
                        continue;

                    foreach (var image in env.Value.Images)
                    {
                        if (image.Repository!=containerRepository)
                            continue;

                        var currentImageTags = _applicationService.GetCurrentImageTags(application, env.Value);
                        
                        var matchingTags = tagCollection.Where(tagDetails => image.Policy.IsMatch(tagDetails.Tag))
                            .ToDictionary(x => x.Tag);
                        
                        var latestTag = matchingTags.Values
                            .OrderBy(tuple => tuple.CreatedAt, Comparer<DateTime>.Default)
                            .Last();

                        var currentTag = currentImageTags[image];
                        
                        if (latestTag.Tag == currentTag)
                        {
                            _log.LogInformation("Latest image tag is applied to the deployment specs");
                        }
                        else
                        {
                            _log.LogInformation(
                                "A new image {latestImageTag} is available for image {imagename} on app {application} (replacing {currentTag})",
                                latestTag.Tag, image.Repository, application.Name, currentTag);
                            await AddDeploymentUpdate(application, env.Value, image, latestTag.Tag);
                        }
                    }
                }
            }
        }

        public async Task AddDeploymentUpdate(Application application, ApplicationEnvironment environment, Image image, string newTag)
        {
            if (!environment.AutoDeploy)
                return;
            
            var currentTags = _applicationService.GetCurrentImageTags(application, environment);

            if (!currentTags.TryGetValue(image, out var currentTag))
            {
                currentTag = "<new image>";
            }
            
            var deploymentUpdate = new DeploymentUpdate(application, environment, image, currentTag, newTag);

            if (!_deploymentUpdates.TryAdd(deploymentUpdate, DeploymentUpdateStatus.Pending))
            {
                _log.LogInformation(
                    "Image tag update operation already in queue for '{Repository}' with {Tag} for application {Application} with new tag {NewTag}",
                    image.Repository,
                    currentTags[image],
                    application.Name,
                    newTag
                );

                return;
            }

            _log.LogInformation(
                "Adding image tag update operation for '{Repository}' with {Tag} for application {Application} with new tag {NewTag}",
                image.Repository, 
                currentTags[image],
                application.Name,
                newTag
            );

            var pendingDeploymentKey = new PendingDeploymentKey(application, environment);
            var queue = _pendingDeploymentUpdates.GetOrAdd(pendingDeploymentKey, key => new ConcurrentQueue<DeploymentUpdate>());
            queue.Enqueue( deploymentUpdate );

            var channel = application.Notifications.Channels.FirstOrDefault();
            if (channel != null)
            {
                _log.LogInformation(
                    "Sending notification about image tag update operation for '{Repository}' with {Tag} for application {Application} with new tag {NewTag}",
                    image.Repository,
                    currentTags[image],
                    application.Name,
                    newTag
                );
                try
                {
                    var handle = await _slackClient.SendDeploymentUpdateNotification(channel, deploymentUpdate, DeploymentUpdateStatus.Pending);
                    _notificationHandles.TryAdd(deploymentUpdate, handle);
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Failed to send deployment update notification to slack");
                }
            }
        }
        
        public async Task ChangeDeploymentUpdateStatus(DeploymentUpdate deploymentUpdate, DeploymentUpdateStatus status)
        {
            _deploymentUpdates[deploymentUpdate] = status;

            if (_notificationHandles.TryGetValue(deploymentUpdate, out var handle))
            {
                try
                {
                    _log.LogInformation("Submitting {@DeploymentUpdate} notification change to slack {@MessageHandle}. ", deploymentUpdate, handle);
                    var newHandle = await _slackClient.UpdateDeploymentUpdateNotification(handle, deploymentUpdate, status);
                    _notificationHandles.TryUpdate(deploymentUpdate, newHandle, handle);
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Failed to submit {@DeploymentUpdate} notification {@MessageHandle}", deploymentUpdate, handle);
                }    
            }
        }
        
        /// <summary>
        ///     Returns the next deployment update in the queue.
        /// </summary>
        /// <returns>Returns the next deployment update in the queue, or <c>null</c> if there are no pending deployment updates.</returns>
        public async Task<DeploymentUpdate> GetNextPendingDeploymentUpdate(Application application, ApplicationEnvironment environment)
        {
            // are there any pending deployments
            if (_pendingDeploymentUpdates.TryGetValue(new PendingDeploymentKey(application, environment), out var queue))
            {
                if (queue.TryDequeue(out var deploymentUpdate))
                {
                    await ChangeDeploymentUpdateStatus(deploymentUpdate, DeploymentUpdateStatus.Starting);
                    return deploymentUpdate;
                }
            }

            return null;
        }

        public async Task FinishDeploymentUpdate(
            DeploymentUpdate deploymentUpdate,
            DeploymentUpdateStatus finalStatus
        )
        {
            await ChangeDeploymentUpdateStatus(deploymentUpdate, finalStatus);
            _applicationService.SetCurrentImageTag(deploymentUpdate.Application, deploymentUpdate.Environment, deploymentUpdate.Image, deploymentUpdate.TargetTag);
        }
    }

    public interface IDeploymentService
    {
        Task AddDeploymentUpdate(string containerRepository, IEnumerable<ImageTag> tags);

        Task AddDeploymentUpdate(Application application, ApplicationEnvironment environment, Image image,
            string newTag);
        
        Task ChangeDeploymentUpdateStatus(DeploymentUpdate deploymentUpdate, DeploymentUpdateStatus status);

        /// <summary>
        ///     Returns the next deployment update in the queue.
        /// </summary>
        /// <returns>Returns the next deployment update in the queue, or <c>null</c> if there are no pending deployment updates.</returns>
        Task<DeploymentUpdate> GetNextPendingDeploymentUpdate(Application application, ApplicationEnvironment environment);

        Task FinishDeploymentUpdate(
            DeploymentUpdate deploymentUpdate,
            DeploymentUpdateStatus finalStatus
        );
    }
}