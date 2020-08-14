using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shipbot.Controller.Core.Apps;
using Shipbot.Controller.Core.Slack;
using Shipbot.Models;

namespace Shipbot.Controller.Core.Deployments
{
    public class DeploymentService : IDeploymentService
    {
        
        
        private readonly IApplicationService _applicationService;

        private readonly ILogger _log;
        
        private readonly ISlackClient _slackClient;
        private readonly IDeploymentQueueService _deploymentQueueService;
        private readonly IDeploymentNotificationService _deploymentNotificationService;
        private readonly DeploymentsDbContext _deploymentsDbContext;

        public DeploymentService(
            ILogger<DeploymentService> log,
            IApplicationService applicationService,
            ISlackClient slackClient,
            IDeploymentQueueService deploymentQueueService,
            IDeploymentNotificationService deploymentNotificationService,
            DeploymentsDbContext deploymentsDbContext
        )
        {
            _log = log;
            _applicationService = applicationService;
            _slackClient = slackClient;
            _deploymentQueueService = deploymentQueueService;
            _deploymentNotificationService = deploymentNotificationService;
            _deploymentsDbContext = deploymentsDbContext;
        }

        protected Task<Dao.Deployment> GetDeploymentDao(DeploymentUpdate deploymentUpdate) =>
            _deploymentsDbContext.Deployments.FirstAsync(x =>
                x.ApplicationId == deploymentUpdate.Application.Name &&
                x.ImageRepository == deploymentUpdate.Image.Repository &&
                x.UpdatePath == deploymentUpdate.Image.TagProperty.Path &&
                x.CurrentImageTag == deploymentUpdate.CurrentTag &&
                x.TargetImageTag == deploymentUpdate.TargetTag
            );

        public async Task AddDeploymentUpdate(Application application, Image image, string newTag)
        {
            var currentTags = _applicationService.GetCurrentImageTags(application);

            if (!currentTags.TryGetValue(image, out var currentTag))
            {
                currentTag = "<new image>";
            }

            var deploymentExists = _deploymentsDbContext.Deployments.Any(
                x =>
                    x.ApplicationId == application.Name &&
                    x.ImageRepository == image.Repository &&
                    x.UpdatePath == image.TagProperty.Path &&
                    x.CurrentImageTag == currentTag &&
                    x.TargetImageTag == newTag
                    );

            if (deploymentExists)
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
            
            await _deploymentsDbContext.Deployments.AddAsync(new Dao.Deployment()
            {
                Id = Guid.NewGuid(),
                ApplicationId = application.Name,
                CreationDateTime = DateTime.Now,
                DeploymentDateTime = null,
                Status = Dao.DeploymentStatus.Pending,
                ImageRepository = image.Repository,
                UpdatePath = image.TagProperty.Path,
                CurrentImageTag = currentTag,
                TargetImageTag = newTag,
                IsAutomaticDeployment = true
            });
            
            var deploymentUpdate = new DeploymentUpdate(
                application, 
                image, 
                currentTag, 
                newTag
                );
            
            _log.LogInformation(
                "Adding image tag update operation for '{Repository}' with {Tag} for application {Application} with new tag {NewTag}",
                image.Repository, 
                currentTags[image],
                application.Name,
                newTag
            );

            await _deploymentQueueService.AddDeployment(application, deploymentUpdate);
            await _deploymentNotificationService.CreateNotification(deploymentUpdate);
            await _deploymentsDbContext.SaveChangesAsync();
        }

        public Task<IEnumerable<Models.DeploymentUpdate>> GetDeployments(Application application)
        {
            var applicationDeploymentDaos = _deploymentsDbContext.Deployments
                .Where(x => x.ApplicationId == application.Name).ToList();

            var imageMap = application.Images.ToDictionary(
                x => $"{x.Repository}-{x.TagProperty.Path}"
            );

            var result = new List<Models.DeploymentUpdate>();

            foreach (var deploymentDao in applicationDeploymentDaos)
            {
                var image = imageMap[$"{deploymentDao.ImageRepository}-{deploymentDao.UpdatePath}"];
                result.Add(
                    new DeploymentUpdate(
                        application, 
                        image, 
                        deploymentDao.CurrentImageTag,
                        deploymentDao.TargetImageTag
                        )
                );
            }

            return Task.FromResult(result.AsEnumerable());
        }

        public async Task ChangeDeploymentUpdateStatus(DeploymentUpdate deploymentUpdate, DeploymentUpdateStatus status)
        {
            var deployment = await GetDeploymentDao(deploymentUpdate);
            deployment.Status = (Dao.DeploymentStatus)status;

            if (status == DeploymentUpdateStatus.Complete)
                deployment.DeploymentDateTime = DateTime.Now;

            await _deploymentNotificationService.UpdateNotification(deploymentUpdate, status);
            await _deploymentsDbContext.SaveChangesAsync();
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