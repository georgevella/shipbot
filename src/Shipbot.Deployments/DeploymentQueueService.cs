using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shipbot.Applications;
using Shipbot.Data;
using Shipbot.Deployments.Dao;
using Shipbot.Models;
using Shipbot.SlackIntegration;
using Deployment = Shipbot.Deployments.Models.Deployment;
using DeploymentStatus = Shipbot.Deployments.Models.DeploymentStatus;

namespace Shipbot.Deployments
{
    public class DeploymentQueueService : IDeploymentQueueService
    {
        private readonly IApplicationService _applicationService;
        private readonly IDeploymentNotificationService _deploymentNotificationService;
        private readonly IEntityRepository<DeploymentQueue> _deploymentQueueRepository;

        public DeploymentQueueService(
            IApplicationService applicationService,
            IDeploymentNotificationService deploymentNotificationService,
            IEntityRepository<DeploymentQueue> deploymentQueueRepository
            )
        {
            _applicationService = applicationService;
            _deploymentNotificationService = deploymentNotificationService;
            _deploymentQueueRepository = deploymentQueueRepository;
        }

        private DeploymentUpdate ConvertFromDao(DeploymentQueue dao)
        {
            var application = _applicationService.GetApplication(dao.ApplicationId);
            var imageMap = application.Images.ToDictionary(
                x => $"{x.Repository}-{x.TagProperty.Path}"
            );
                
            var image = imageMap[$"{dao.Deployment.ImageRepository}-{dao.Deployment.UpdatePath}"];
                
            var deploymentUpdate = new DeploymentUpdate(
                dao.DeploymentId,
                application, 
                image, 
                dao.Deployment.CurrentImageTag, 
                dao.Deployment.TargetImageTag
            );

            return deploymentUpdate;
        }
        
        public async Task AddDeployment(Deployment deployment)
        {
            if (deployment.Status != DeploymentStatus.Pending)
                return;
            
            // add entry in store
            var dao = await _deploymentQueueRepository.Add(new DeploymentQueue()
            {
                DeploymentId = deployment.Id,
                Id = Guid.NewGuid(),
                ApplicationId = deployment.ApplicationId,
                AttemptCount = 0,
                AvailableDateTime = DateTime.Now.AddMinutes(10),
                CreationDateTime = DateTime.Now
            });

            await _deploymentQueueRepository.Save();
            
            await _deploymentNotificationService.CreateNotification(ConvertFromDao(dao));
        }

        public async Task<DeploymentUpdate?> GetNextPendingDeploymentUpdate(Application application)
        {
            // are there any pending deployments
            var queue = _deploymentQueueRepository.Query()
                .Where(
                    x => x.ApplicationId == application.Name
                         && x.AcknowledgeDateTime == null
                         && x.AvailableDateTime <= DateTime.Now
                         )
                .OrderBy(x => x.AvailableDateTime)
                .ToList();

            if (!queue.Any())
                return null;

            var first = queue.First();
            first.AcknowledgeDateTime = DateTime.Now;
            first = _deploymentQueueRepository.Update(first);

            await _deploymentQueueRepository.Save();

            return ConvertFromDao(first);
        }

        public Task<IEnumerable<DeploymentUpdate>> GetPendingDeployments()
        {
            var queue = _deploymentQueueRepository.Query()
                .Where(
                    x => x.AcknowledgeDateTime == null
                )
                .OrderBy(x => x.AvailableDateTime)
                .ToList();

            var allPendingDeployments = queue.Select(ConvertFromDao).ToList();
            return Task.FromResult(allPendingDeployments.AsEnumerable());
        }
    }
}