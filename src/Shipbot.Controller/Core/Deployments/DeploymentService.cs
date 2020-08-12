using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Shipbot.Controller.Core.ApplicationSources;
using Shipbot.Controller.Core.Apps;
using Shipbot.Controller.Core.Models;
using Shipbot.Controller.Core.Slack;

namespace Shipbot.Controller.Core.Deployments
{
    public class DeploymentService : IDeploymentService
    {
        private readonly ConcurrentDictionary<DeploymentUpdate, DeploymentUpdateStatus> _deploymentUpdates = new ConcurrentDictionary<DeploymentUpdate, DeploymentUpdateStatus>();
        
        private readonly ConcurrentDictionary<Application, ConcurrentQueue<DeploymentUpdate>> _pendingDeploymentUpdates = new ConcurrentDictionary<Application, ConcurrentQueue<DeploymentUpdate>>();
        
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
        
        public async Task AddDeploymentUpdate(Application application, Image image, string newTag)
        {
            var currentTags = _applicationService.GetCurrentImageTags(application);

            if (!currentTags.TryGetValue(image, out var currentTag))
            {
                currentTag = "<new image>";
            }
            
            var deploymentUpdate = new DeploymentUpdate(application, image, currentTag, newTag);

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
            var queue = _pendingDeploymentUpdates.GetOrAdd(application, key => new ConcurrentQueue<DeploymentUpdate>());
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
        public async Task<DeploymentUpdate> GetNextPendingDeploymentUpdate(Application application)
        {
            // are there any pending deployments
            if (_pendingDeploymentUpdates.TryGetValue(application, out var queue))
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
            _applicationService.SetCurrentImageTag(deploymentUpdate.Application, deploymentUpdate.Image, deploymentUpdate.TargetTag);
        }
    }
}