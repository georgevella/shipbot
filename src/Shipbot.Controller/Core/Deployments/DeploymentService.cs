using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using Shipbot.Controller.Core.ApplicationSources;
using Shipbot.Controller.Core.Apps;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.Core.Models;
using Shipbot.Controller.Core.Registry.Watcher;
using Shipbot.Controller.Core.Slack;

namespace Shipbot.Controller.Core.Deployments
{
    public class DeploymentService : IDeploymentService
    {
        private readonly ConcurrentDictionary<DeploymentKey, Deployment> _deployments = new ConcurrentDictionary<DeploymentKey, Deployment>();
        
        /// <summary>
        ///     queue used by deployment source updaters to pick up the next job
        /// </summary>
        private readonly ConcurrentDictionary<PendingDeploymentKey, ConcurrentQueue<DeploymentUpdate>> _pendingDeploymentUpdates = new ConcurrentDictionary<PendingDeploymentKey, ConcurrentQueue<DeploymentUpdate>>();
        
        private readonly ConcurrentDictionary<DeploymentKey, IMessageHandle> _notificationHandles = new ConcurrentDictionary<DeploymentKey, IMessageHandle>();
        
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
            
            var deploymentKey = new DeploymentKey(application, image, newTag);
            var deployment = new Deployment(deploymentKey);

            if (!_deployments.TryAdd(deploymentKey, deployment))
            {
                _log.LogInformation(
                    "Image tag deployment already known for '{Repository}' with {Tag} for application {Application} with new tag {NewTag}",
                    image.Repository,
                    currentTags[image],
                    application.Name,
                    newTag
                );

                return;
            }
            
            if (environment.AutoDeploy)
            {
                // we will create a DeploymentUpdate automatically for environments that don't have AutoDeploy set to on.
                var deploymentUpdate = new DeploymentUpdate(application, environment, image, currentTag, newTag);
                
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

                deployment.AddDeploymentUpdate(deploymentUpdate);
            }
            
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
                    var handle = await _slackClient.SendDeploymentUpdateNotification(channel, deployment);
                    _notificationHandles.TryAdd(deploymentKey, handle);
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Failed to send deployment update notification to slack");
                }
            }
        }
        
        public async Task ChangeDeploymentUpdateStatus(DeploymentUpdate deploymentUpdate, DeploymentUpdateStatus status)
        {
            var deploymentKey = new DeploymentKey(deploymentUpdate.Application, deploymentUpdate.Image, deploymentUpdate.TargetTag);

            if (!_deployments.TryGetValue(deploymentKey, out var deployment))
            {
                throw new InvalidOperationException("Deployment update for an untracked deployment");
            }

            deployment.ChangeDeploymentUpdateStatus(deploymentUpdate, status);

            if (_notificationHandles.TryGetValue(deploymentKey, out var handle))
            {
                try
                {
                    _log.LogInformation("Submitting {@DeploymentUpdate} notification change to slack {@MessageHandle}. ", deploymentUpdate, handle);
                    var newHandle = await _slackClient.UpdateDeploymentUpdateNotification(handle, deployment);
                    _notificationHandles.TryUpdate(deploymentKey, newHandle, handle);
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
            if (deploymentUpdate.IsTriggeredByPromotion)
            {
                await ChangeDeploymentUpdateStatus(deploymentUpdate.SourceDeploymentUpdate,
                    DeploymentUpdateStatus.Promoted);
            }
            
            await ChangeDeploymentUpdateStatus(deploymentUpdate, finalStatus);
            
            // ensure application service knows about the updated image tag
            _applicationService.SetCurrentImageTag(deploymentUpdate.Application, deploymentUpdate.Environment, deploymentUpdate.Image, deploymentUpdate.TargetTag);
        }

        public async Task PromoteDeployment(Application application, string containerRepository, string targetTag,
            string sourceEnvironment)
        {
            var deploymentKey = new DeploymentKey(application, containerRepository, targetTag);
            
            if (_deployments.TryGetValue(deploymentKey, out var deployment))
            {
                var deploymentUpdateDetails = deployment.GetDeploymentUpdates()
                    .First(x => x.DeploymentUpdate.Environment.Name == sourceEnvironment);
                
                DoPromoteDeployment(deploymentUpdateDetails.DeploymentUpdate, deployment);
            }
        }

        public async Task PromoteDeployment(DeploymentUpdate deploymentUpdate)
        {
            var deploymentKey = new DeploymentKey(deploymentUpdate.Application, deploymentUpdate.Image, deploymentUpdate.TargetTag);

            if (_deployments.TryGetValue(deploymentKey, out var deployment))
            {
                DoPromoteDeployment(deploymentUpdate, deployment);
            }
        }

        private void DoPromoteDeployment(DeploymentUpdate deploymentUpdate, Deployment deployment)
        {
            deployment.ChangeDeploymentUpdateStatus(deploymentUpdate, DeploymentUpdateStatus.Promoting);

            var targetEnvironmentName = deploymentUpdate.Environment.PromotionEnvironments.First();
            var environment = deploymentUpdate.Application.Environments[targetEnvironmentName];
            var currentTags = _applicationService.GetCurrentImageTags(deploymentUpdate.Application, environment);

            if (!currentTags.TryGetValue(deploymentUpdate.Image, out var currentTag))
            {
                currentTag = "<new image>";
            }

            var newDeploymentUpdate = deployment.CreateDeploymentUpdate(environment, currentTag,
                deploymentUpdate.TargetTag, deploymentUpdate);

            _log.LogInformation(
                "Adding image tag update operation for '{Repository}' with {Tag} for application {Application} with new tag {NewTag}",
                newDeploymentUpdate.Image.Repository,
                currentTag,
                deploymentUpdate.Application.Name,
                newDeploymentUpdate.TargetTag
            );

            var pendingDeploymentKey = new PendingDeploymentKey(newDeploymentUpdate.Application, environment);
            var queue = _pendingDeploymentUpdates.GetOrAdd(pendingDeploymentKey,
                key => new ConcurrentQueue<DeploymentUpdate>());
            queue.Enqueue(newDeploymentUpdate);

            deployment.AddDeploymentUpdate(newDeploymentUpdate);
        }
    }
}