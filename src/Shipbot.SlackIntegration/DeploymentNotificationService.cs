using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shipbot.Models;
using Shipbot.SlackIntegration.Internal;

namespace Shipbot.SlackIntegration
{
    public class DeploymentNotificationService : IDeploymentNotificationService
    {
        private readonly ILogger<DeploymentNotificationService> _log;
        private readonly IDeploymentNotificationBuilder _deploymentNotificationBuilder;
        private readonly ISlackClient _slackClient;
        private static readonly ConcurrentDictionary<DeploymentUpdate, IMessageHandle> _notificationHandles = new ConcurrentDictionary<DeploymentUpdate, IMessageHandle>();

        public DeploymentNotificationService(
            ILogger<DeploymentNotificationService> log,
            IDeploymentNotificationBuilder deploymentNotificationBuilder,
            ISlackClient slackClient
        )
        {
            _log = log;
            _deploymentNotificationBuilder = deploymentNotificationBuilder;
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
                    var notification =
                        _deploymentNotificationBuilder.BuildNotification(deploymentUpdate,
                            DeploymentUpdateStatus.Pending);
                    
                    var handle = await _slackClient.PostMessageAsync(channel, notification);
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
                    var notification = _deploymentNotificationBuilder.BuildNotification(deploymentUpdate, status);
                    var newHandle = await _slackClient.UpdateMessageAsync(handle, notification);
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