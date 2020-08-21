using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shipbot.Contracts;
using Shipbot.Models;
using Shipbot.SlackIntegration;

namespace Shipbot.Controller.Core.Deployments
{
    public class DeploymentNotificationService : IDeploymentNotificationService
    {
        private readonly ILogger<DeploymentNotificationService> _log;
        private readonly ISlackClient _slackClient;
        private readonly ConcurrentDictionary<DeploymentUpdate, IMessageHandle> _notificationHandles = new ConcurrentDictionary<DeploymentUpdate, IMessageHandle>();

        public DeploymentNotificationService(
            ILogger<DeploymentNotificationService> log,
            ISlackClient slackClient
        )
        {
            _log = log;
            _slackClient = slackClient;
        }
        
        public async Task CreateNotification(DeploymentUpdate deploymentUpdate)
        {
            var channel = deploymentUpdate.Application.Notifications.Channels.FirstOrDefault();
            
            if (channel != null)
            {
                _log.LogInformation(
                    "Sending notification about image tag update operation for '{Repository}' with {Tag} for application {Application} with new tag {NewTag}",
                    deploymentUpdate.Image.Repository,
                    deploymentUpdate.CurrentTag,
                    deploymentUpdate.Application.Name,
                    deploymentUpdate.TargetTag
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

        public async Task UpdateNotification(DeploymentUpdate deploymentUpdate, DeploymentUpdateStatus status)
        {
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
    }
}