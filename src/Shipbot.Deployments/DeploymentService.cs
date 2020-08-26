using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shipbot.Applications;
using Shipbot.Data;
using Shipbot.Deployments.Models;
using Shipbot.Models;
using Shipbot.SlackIntegration;
using Deployment = Shipbot.Deployments.Models.Deployment;

namespace Shipbot.Deployments
{
    public class DeploymentService : IDeploymentService
    {
        private readonly IApplicationService _applicationService;

        private readonly ILogger _log;
        private readonly IDeploymentQueueService _deploymentQueueService;
        private readonly IDeploymentNotificationService _deploymentNotificationService;
        private readonly ShipbotDbContext _deploymentsDbContextConfigurator;

        public DeploymentService(
            ILogger<DeploymentService> log,
            IApplicationService applicationService,
            IDeploymentQueueService deploymentQueueService,
            IDeploymentNotificationService deploymentNotificationService,
            ShipbotDbContext deploymentsDbContextConfigurator
        )
        {
            _log = log;
            _applicationService = applicationService;
            _deploymentQueueService = deploymentQueueService;
            _deploymentNotificationService = deploymentNotificationService;
            _deploymentsDbContextConfigurator = deploymentsDbContextConfigurator;
        }

        private Task<Dao.Deployment> GetDeploymentDao(DeploymentUpdate deploymentUpdate)
        {
            var deployments = _deploymentsDbContextConfigurator.Set<Dao.Deployment>();
            
            return deployments.FirstAsync(x =>
                x.ApplicationId == deploymentUpdate.Application.Name &&
                x.ImageRepository == deploymentUpdate.Image.Repository &&
                x.UpdatePath == deploymentUpdate.Image.TagProperty.Path &&
                x.CurrentImageTag == deploymentUpdate.CurrentTag &&
                x.TargetImageTag == deploymentUpdate.TargetTag
            );
        }

        private static Deployment ConvertFromDao(Dao.Deployment deploymentDao) =>
            new Deployment(
                deploymentDao.Id,
                deploymentDao.ApplicationId,
                deploymentDao.ImageRepository,
                deploymentDao.UpdatePath,
                deploymentDao.CurrentImageTag,
                deploymentDao.TargetImageTag,
                (DeploymentStatus) deploymentDao.Status);


        public async Task<Deployment> AddDeployment(Application application, Image image, string newTag)
        {
            var currentTags = _applicationService.GetCurrentImageTags(application);

            if (!currentTags.TryGetValue(image, out var currentTag))
            {
                currentTag = "<new image>";
            }
            
            var deployments = _deploymentsDbContextConfigurator.Set<Dao.Deployment>();
            
            var deploymentExists = deployments.Any(
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

                throw new Exception($"Deployment for {image} with {newTag} already exists.");
            }
            
            var entity = await deployments.AddAsync(new Dao.Deployment()
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

            _log.LogInformation(
                "Adding image tag update operation for '{Repository}' with {Tag} for application {Application} with new tag {NewTag}",
                image.Repository, 
                currentTags[image],
                application.Name,
                newTag
            );
            
            await _deploymentsDbContextConfigurator.SaveChangesAsync();

            var deployment = ConvertFromDao(entity.Entity);
            
            if (application.AutoDeploy)
            {
                await _deploymentQueueService.AddDeployment(deployment);
            }

            return deployment;
        }

        public Task<IEnumerable<Deployment>> GetDeployments(Application application)
        {
            var deployments = _deploymentsDbContextConfigurator.Set<Dao.Deployment>();
            
            var applicationDeploymentDaos = deployments
                .Where(x => x.ApplicationId == application.Name).ToList();

            var imageMap = application.Images.ToDictionary(
                x => $"{x.Repository}-{x.TagProperty.Path}"
            );

            var result = new List<Deployment>();

            foreach (var deploymentDao in applicationDeploymentDaos)
            {
                var image = imageMap[$"{deploymentDao.ImageRepository}-{deploymentDao.UpdatePath}"];
                result.Add(ConvertFromDao(deploymentDao));
            }

            return Task.FromResult(result.AsEnumerable());
        }

        public async Task<Deployment> GetDeployment(Guid deploymentId)
        {
            var deployments = _deploymentsDbContextConfigurator.Set<Dao.Deployment>();
            var deploymentDao = await deployments.FindAsync(deploymentId);
            return ConvertFromDao(deploymentDao);
        }

        public async Task ChangeDeploymentUpdateStatus(Guid deploymentId, DeploymentUpdateStatus status)
        {
            try
            {
                var deployments = _deploymentsDbContextConfigurator.Set<Dao.Deployment>();
                var deploymentDao = await deployments.FindAsync(deploymentId);
                var application = _applicationService.GetApplication(deploymentDao.ApplicationId);
                
                var imageMap = application.Images.ToDictionary(
                    x => $"{x.Repository}-{x.TagProperty.Path}"
                );
                var image = imageMap[$"{deploymentDao.ImageRepository}-{deploymentDao.UpdatePath}"];
                
                deploymentDao.Status = (Dao.DeploymentStatus) status;

                if (status == DeploymentUpdateStatus.Complete)
                    deploymentDao.DeploymentDateTime = DateTime.Now;
                
                
                // send notification out
                var deploymentUpdate = new DeploymentUpdate(
                    deploymentDao.Id,
                    application, 
                    image, 
                    deploymentDao.CurrentImageTag, 
                    deploymentDao.TargetImageTag
                );
                
                await _deploymentNotificationService.UpdateNotification(deploymentUpdate, status);
                await _deploymentsDbContextConfigurator.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _log.LogError("Failed to update deployment information", e);
                throw;
            }
        }

        public async Task FinishDeploymentUpdate(
            Guid deploymentId,
            DeploymentUpdateStatus finalStatus
        )
        {
            await ChangeDeploymentUpdateStatus(deploymentId, finalStatus);
            
            var deployments = _deploymentsDbContextConfigurator.Set<Dao.Deployment>();
            var deploymentDao = await deployments.FindAsync(deploymentId);
            var application = _applicationService.GetApplication(deploymentDao.ApplicationId);
            var imageMap = application.Images.ToDictionary(
                x => $"{x.Repository}-{x.TagProperty.Path}"
            );
            var image = imageMap[$"{deploymentDao.ImageRepository}-{deploymentDao.UpdatePath}"];
            
            // TODO: trigger git refresh instead of explicitly setting the tag
            _applicationService.SetCurrentImageTag(application, image, deploymentDao.TargetImageTag);
        }
    }
}