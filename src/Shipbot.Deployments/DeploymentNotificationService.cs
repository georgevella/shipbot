using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shipbot.Applications;
using Shipbot.Deployments.Models;
using Shipbot.Models;
using Shipbot.SlackIntegration;

namespace Shipbot.Deployments
{
    public class DeploymentNotificationService : IDeploymentNotificationService
    {
        private static readonly ConcurrentDictionary<Guid, IMessageHandle> NotificationHandles = new ConcurrentDictionary<Guid, IMessageHandle>();
        
        private readonly ILogger<DeploymentNotificationService> _log;
        private readonly IDeploymentNotificationBuilder _deploymentNotificationBuilder;
        private readonly IApplicationService _applicationService;
        private readonly ISlackClient _slackClient;

        public DeploymentNotificationService(
            ILogger<DeploymentNotificationService> log,
            IDeploymentNotificationBuilder deploymentNotificationBuilder,
            IApplicationService applicationService,
            ISlackClient slackClient
        )
        {
            _log = log;
            _deploymentNotificationBuilder = deploymentNotificationBuilder;
            _applicationService = applicationService;
            _slackClient = slackClient;
        }
        
        public async Task CreateNotification(Deployment deployment)
        {
            var application = _applicationService.GetApplication(deployment.ApplicationId);
            var channel = application.Notifications.Channels.FirstOrDefault();
            
            if (channel != null)
            {
                _log.LogInformation(
                    "Sending notification about image tag update operation for '{Repository}' with {Tag} for application {Application} with new tag {NewTag}",
                    deployment.ImageRepository,
                    deployment.CurrentTag,
                    deployment.ApplicationId,
                    deployment.TargetTag
                );
                try
                {
                    var notification =
                        _deploymentNotificationBuilder.BuildNotification(
                            deployment
                            );
                    
                    var handle = await _slackClient.PostMessageAsync(channel, notification);
                    NotificationHandles.TryAdd(deployment.Id, handle);
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Failed to send deployment update notification to slack");
                }
            }
        }

        public async Task UpdateNotification(Deployment deployment)
        {
            if (NotificationHandles.TryGetValue(deployment.Id, out var handle))
            {
                try
                {
                    _log.LogInformation("Submitting {@DeploymentUpdate} notification change to slack {@MessageHandle}. ", deployment, handle);
                    var notification = _deploymentNotificationBuilder.BuildNotification(deployment);
                    var newHandle = await _slackClient.UpdateMessageAsync(handle, notification);
                    NotificationHandles.TryUpdate(deployment.Id, newHandle, handle);
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Failed to submit {@DeploymentUpdate} notification {@MessageHandle}", deployment, handle);
                }    
            }
        }
    }
    
    public interface IDeploymentNotificationService
    {
        Task CreateNotification(Deployment deployment);
        Task UpdateNotification(Deployment deployment);
    }
}