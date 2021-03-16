using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Shipbot.Applications;
using Shipbot.Applications.Models;
using Shipbot.Data;
using Shipbot.Deployments.Internals;
using Shipbot.Deployments.Models;
using Deployment = Shipbot.Deployments.Models.Deployment;
using DaoDeploymentType = Shipbot.Deployments.Dao.DeploymentType;

namespace Shipbot.Deployments
{
    public class DeploymentService : IDeploymentService
    {
        private readonly IApplicationService _applicationService;
        private readonly IApplicationImageInstanceService _applicationImageInstanceService;

        private readonly ILogger _log;
        private readonly IDeploymentNotificationService _deploymentNotificationService;
        private readonly ShipbotDbContext _deploymentsDbContextConfigurator;

        public DeploymentService(
            ILogger<DeploymentService> log,
            IApplicationService applicationService,
            IApplicationImageInstanceService applicationImageInstanceService,
            IDeploymentNotificationService deploymentNotificationService,
            ShipbotDbContext deploymentsDbContextConfigurator
        )
        {
            _log = log;
            _applicationService = applicationService;
            _applicationImageInstanceService = applicationImageInstanceService;
            _deploymentNotificationService = deploymentNotificationService;
            _deploymentsDbContextConfigurator = deploymentsDbContextConfigurator;
        }

        public async Task<Deployment> AddDeployment(
            Application application,
            ApplicationImage image,
            string newTag,
            DeploymentType type = DeploymentType.ImageUpdate,
            string instanceId = "",
            IReadOnlyDictionary<string, string>? parameters = null)
        {
            var currentTagInStore = await _applicationImageInstanceService.GetCurrentTag(application, image, instanceId);
            var currentTag = currentTagInStore.available ? currentTagInStore.tag : "";

            var deployments = _deploymentsDbContextConfigurator.Set<Dao.Deployment>();

            var deploymentExists = await IsDeploymentPresent(
                application,
                image,
                newTag,
                type,
                instanceId
            );

            if (deploymentExists)
            {
                _log.LogInformation(
                    "Image tag update operation already in queue for '{Repository}' with {Tag} for application {Application} with new tag {NewTag}",
                    image.Repository,
                    currentTag,
                    application.Name,
                    newTag
                );

                throw new Exception($"Deployment for {image} with {newTag} on application '{application.Name}' already exists.");
            }

            var entity = await CreateAndStoreDeploymentDao(
                deployments,
                application,
                image,
                type == DeploymentType.ImageUpdate ? DaoDeploymentType.ImageUpdate : DaoDeploymentType.PreviewRelease,
                newTag,
                currentTag,
                string.Empty,
                instanceId,
                parameters
            );

            _log.LogInformation(
                "Adding image tag update operation for '{Repository}' with {Tag} for application {Application} with new tag {NewTag}",
                image.Repository, 
                currentTag,
                application.Name,
                newTag
            );
            
            await _deploymentsDbContextConfigurator.SaveChangesAsync();

            var deployment = entity.Entity.ConvertToDeploymentModel();

            await _deploymentNotificationService.CreateNotification(deployment);

            return deployment;
        }

        private static async Task<EntityEntry<Dao.Deployment>> CreateAndStoreDeploymentDao(
            DbSet<Dao.Deployment> deployments,
            Application application, 
            ApplicationImage image,
            DaoDeploymentType type,
            string newTag,
            string currentTag = "",
            string nameSuffix = "",
            string instanceId = "", 
            IReadOnlyDictionary<string, string>? parameters = null)
        {
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
                IsAutomaticDeployment = true,
                Type = type,
                NameSuffix = nameSuffix,
                InstanceId = instanceId,
                Parameters = new Dictionary<string, string>(parameters ?? new Dictionary<string, string>())
            });
            return entity;
        }

        public Task<bool> IsDeploymentPresent( 
            Application application, 
            ApplicationImage image, 
            string newTag, 
            DeploymentType type = DeploymentType.ImageUpdate,
            string instanceId = "")
        {
            var deployments = _deploymentsDbContextConfigurator.Set<Dao.Deployment>();
            
            return deployments.AnyAsync(
                x =>
                    x.ApplicationId == application.Name &&
                    x.ImageRepository == image.Repository &&
                    x.UpdatePath == image.TagProperty.Path &&
                    x.TargetImageTag == newTag &&
                    x.Type == (type == DeploymentType.ImageUpdate ? DaoDeploymentType.ImageUpdate : DaoDeploymentType.PreviewRelease ) &&
                    x.InstanceId == instanceId
            );
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
                _log.LogError(e, "Failed to update deployment information");
                throw;
            }
        }

        public async Task FinishDeploymentUpdate(
            Guid deploymentId,
            DeploymentStatus finalStatus
        )
        {
            await ChangeDeploymentUpdateStatus(deploymentId, finalStatus);
            
            var deploymentsDbSet = _deploymentsDbContextConfigurator.Set<Dao.Deployment>();
            var deploymentDao = await deploymentsDbSet.FindAsync(deploymentId);
            var application = _applicationService.GetApplication(deploymentDao.ApplicationId);
            var imageMap = application.Images.ToDictionary(
                x => $"{x.Repository}-{x.TagProperty.Path}"
            );
            var image = imageMap[$"{deploymentDao.ImageRepository}-{deploymentDao.UpdatePath}"];
            
            // TODO: trigger git refresh instead of explicitly setting the tag
            await _applicationImageInstanceService.SetCurrentTag(application, image, deploymentDao.InstanceId, deploymentDao.TargetImageTag);
        }
    }
}