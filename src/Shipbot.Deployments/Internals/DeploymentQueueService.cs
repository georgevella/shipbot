using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shipbot.Applications;
using Shipbot.Data;
using Shipbot.Deployments.Dao;
using Shipbot.Deployments.Internals;
using Shipbot.Models;
using Shipbot.SlackIntegration;
using Deployment = Shipbot.Deployments.Models.Deployment;
using DeploymentStatus = Shipbot.Deployments.Models.DeploymentStatus;

namespace Shipbot.Deployments
{
    public class DeploymentQueueService : IDeploymentQueueService
    {
        private readonly IApplicationService _applicationService;
        private readonly IDeploymentService _deploymentService;
        private readonly IDeploymentNotificationService _deploymentNotificationService;
        private readonly IEntityRepository<DeploymentQueue> _deploymentQueueRepository;

        public DeploymentQueueService(
            IApplicationService applicationService,
            IDeploymentService deploymentService,
            IDeploymentNotificationService deploymentNotificationService,
            IEntityRepository<DeploymentQueue> deploymentQueueRepository
            )
        {
            _applicationService = applicationService;
            _deploymentService = deploymentService;
            _deploymentNotificationService = deploymentNotificationService;
            _deploymentQueueRepository = deploymentQueueRepository;
        }

        // private Deployment ConvertFromDao(DeploymentQueue dao)
        // {
        //     var application = _applicationService.GetApplication(dao.ApplicationId);
        //     var imageMap = application.Images.ToDictionary(
        //         x => $"{x.Repository}-{x.TagProperty.Path}"
        //     );
        //         
        //     var image = imageMap[$"{dao.Deployment.ImageRepository}-{dao.Deployment.UpdatePath}"];
        //
        //     var deployment = dao.Deployment.ConvertToDeploymentModel();
        //
        //     return deployment;
        // }
        
        public async Task EnqueueDeployment(Deployment deployment, TimeSpan? delay = null)
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
                AvailableDateTime = DateTime.Now.Add(delay ?? TimeSpan.FromSeconds(0)),
                CreationDateTime = DateTime.Now
            });
            
            await _deploymentQueueRepository.Save();
            
            await _deploymentService.ChangeDeploymentUpdateStatus(deployment.Id, DeploymentStatus.Queued);
        }

        public async Task<Deployment?> GetNextPendingDeploymentUpdate(Application application)
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

            return first.Deployment.ConvertToDeploymentModel();
        }

        public Task<IEnumerable<Deployment>> GetPendingDeployments()
        {
            var queue = _deploymentQueueRepository.Query()
                .Where(
                    x => x.AcknowledgeDateTime == null
                )
                .OrderBy(x => x.AvailableDateTime)
                .ToList();

            var allPendingDeployments = queue.Select(x=>x.Deployment.ConvertToDeploymentModel()).ToList();
            return Task.FromResult(allPendingDeployments.AsEnumerable());
        }
    }
}