using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shipbot.Applications;
using Shipbot.Data;
using Shipbot.Deployments.Dao;
using Shipbot.Models;
using Shipbot.SlackIntegration;
using Shipbot.SlackIntegration.Internal;
using Deployment = Shipbot.Deployments.Models.Deployment;

namespace Shipbot.Deployments
{
    public class DeploymentNotificationService : IDeploymentNotificationService
    {
        private readonly ILogger<DeploymentNotificationService> _log;
        private readonly IDeploymentNotificationBuilder _deploymentNotificationBuilder;
        private readonly IApplicationService _applicationService;
        private readonly IEntityRepository<DeploymentNotification> _deploymentNotificationRepository;
        private readonly ISlackClient _slackClient;

        public DeploymentNotificationService(
            ILogger<DeploymentNotificationService> log,
            IDeploymentNotificationBuilder deploymentNotificationBuilder,
            IApplicationService applicationService,
            IEntityRepository<DeploymentNotification> deploymentNotificationRepository,
            ISlackClient slackClient
        )
        {
            _log = log;
            _deploymentNotificationBuilder = deploymentNotificationBuilder;
            _applicationService = applicationService;
            _deploymentNotificationRepository = deploymentNotificationRepository;
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

                    await _deploymentNotificationRepository.Add(new DeploymentNotification()
                    {
                        Id = Guid.NewGuid(),
                        DeploymentId = deployment.Id,
                        SlackMessageId = handle.Id
                    });

                    await _deploymentNotificationRepository.Save();
                    
                    //NotificationHandles.TryAdd(deployment.Id, handle);
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Failed to send deployment update notification to slack");
                }
            }
        }

        public async Task UpdateNotification(Deployment deployment)
        {
            var deploymentNotificationDao = await _deploymentNotificationRepository.Query()
                .FirstOrDefaultAsync(x => x.DeploymentId == deployment.Id);
            if (deploymentNotificationDao != null)
            {
                var handle = new MessageHandle(deploymentNotificationDao.SlackMessageId);

                try
                {
                    _log.LogInformation("Submitting {@DeploymentUpdate} notification change to slack {@MessageHandle}. ", deployment, handle);
                    var notification = _deploymentNotificationBuilder.BuildNotification(deployment);
                    var newHandle = await _slackClient.UpdateMessageAsync(handle, notification);
                    // NotificationHandles.TryUpdate(deployment.Id, newHandle, handle);
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