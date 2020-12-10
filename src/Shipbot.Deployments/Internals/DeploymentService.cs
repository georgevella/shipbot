using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shipbot.Applications;
using Shipbot.Data;
using Shipbot.Deployments.Internals;
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
        private readonly IDeploymentNotificationService _deploymentNotificationService;
        private readonly ShipbotDbContext _deploymentsDbContextConfigurator;

        public DeploymentService(
            ILogger<DeploymentService> log,
            IApplicationService applicationService,
            IDeploymentNotificationService deploymentNotificationService,
            ShipbotDbContext deploymentsDbContextConfigurator
        )
        {
            _log = log;
            _applicationService = applicationService;
            _deploymentNotificationService = deploymentNotificationService;
            _deploymentsDbContextConfigurator = deploymentsDbContextConfigurator;
        }
        
        public async Task<Deployment> AddDeployment(Application application, ApplicationImage image, string newTag)
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

            var deployment = entity.Entity.ConvertToDeploymentModel();
            
            await _deploymentNotificationService.CreateNotification(deployment);

            return deployment;
        }

        public Task<IEnumerable<Deployment>> GetDeployments(Application? application, DeploymentStatus? status)
        {
            var deploymentsDbSet = _deploymentsDbContextConfigurator.Set<Dao.Deployment>();

            var query = (IQueryable<Dao.Deployment>) deploymentsDbSet;

            if (application != null)
            {
                query = query.Where(x => x.ApplicationId == application.Name);
            }

            if (status != null)
            {
                // NOTE: the conversion from Models.DeploymentStatus to Dao.DeploymentStatus is done 
                // here outside of the Where clause due to a bug in EFCore as described in this
                // post: https://stackoverflow.com/questions/55182602/efcore-enum-to-string-value-conversion-not-used-in-where-clause
                
                var daoStatus = (Dao.DeploymentStatus) status;
                query = query.Where(x => x.Status == daoStatus);
            }
            
            var applicationDeploymentDaos = query.ToList();

            // var imageMap = application.Images.ToDictionary(
            //     x => $"{x.Repository}-{x.TagProperty.Path}"
            // );

            var result = new List<Deployment>();

            foreach (var deploymentDao in applicationDeploymentDaos)
            {
                // var image = imageMap[$"{deploymentDao.ImageRepository}-{deploymentDao.UpdatePath}"];
                result.Add(deploymentDao.ConvertToDeploymentModel());
            }

            return Task.FromResult(result.AsEnumerable());
        }
        
        public async Task<Deployment> GetDeployment(Guid deploymentId)
        {
            var deployments = _deploymentsDbContextConfigurator.Set<Dao.Deployment>();
            var deploymentDao = await deployments.FindAsync(deploymentId);
            return deploymentDao.ConvertToDeploymentModel();
        }

        public async Task ChangeDeploymentUpdateStatus(Guid deploymentId, DeploymentStatus status)
        {
            try
            {
                var deployments = _deploymentsDbContextConfigurator.Set<Dao.Deployment>();
                var deploymentDao = await deployments.FindAsync(deploymentId);
                
                deploymentDao.Status = (Dao.DeploymentStatus) status;
                
                if (status == DeploymentStatus.Complete)
                    deploymentDao.DeploymentDateTime = DateTime.Now;

                await _deploymentsDbContextConfigurator.SaveChangesAsync();
                await _deploymentNotificationService.UpdateNotification(deploymentDao.ConvertToDeploymentModel());
            }
            catch (Exception e)
            {
                _log.LogError("Failed to update deployment information", e);
                throw;
            }
        }

        public async Task FinishDeploymentUpdate(
            Guid deploymentId,
            DeploymentStatus finalStatus
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