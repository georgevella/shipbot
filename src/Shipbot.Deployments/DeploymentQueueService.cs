using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shipbot.Applications;
using Shipbot.Deployments.Models;
using Shipbot.Models;
using Shipbot.SlackIntegration;

namespace Shipbot.Deployments
{
    public class DeploymentQueueService : IDeploymentQueueService
    {
        private static readonly ConcurrentDictionary<Application, ConcurrentQueue<DeploymentUpdate>> PendingDeploymentUpdates = new ConcurrentDictionary<Application, ConcurrentQueue<DeploymentUpdate>>();

        private readonly IApplicationService _applicationService;
        private readonly IDeploymentNotificationService _deploymentNotificationService;

        public DeploymentQueueService(
            IApplicationService applicationService,
            IDeploymentNotificationService deploymentNotificationService
            )
        {
            _applicationService = applicationService;
            _deploymentNotificationService = deploymentNotificationService;
        }
        
        public async Task AddDeployment(Deployment deployment)
        {
            var application = _applicationService.GetApplication(deployment.ApplicationId);
            var imageMap = application.Images.ToDictionary(
                x => $"{x.Repository}-{x.TagProperty.Path}"
            );
                
            var image = imageMap[$"{deployment.ImageRepository}-{deployment.UpdatePath}"];
                
            var deploymentUpdate = new DeploymentUpdate(
                deployment.Id,
                application, 
                image, 
                deployment.CurrentTag, 
                deployment.TargetTag
            );
                
            var queue = PendingDeploymentUpdates.GetOrAdd(
                application,
                key => new ConcurrentQueue<DeploymentUpdate>()
            );
                
            queue.Enqueue(deploymentUpdate);
                
            await _deploymentNotificationService.CreateNotification(deploymentUpdate);
        }

        public Task<DeploymentUpdate?> GetNextPendingDeploymentUpdate(Application application)
        {
            // are there any pending deployments
            if (!PendingDeploymentUpdates.TryGetValue(application, out var queue))
                return Task.FromResult<DeploymentUpdate?>(null);
            
            return queue.TryDequeue(out var deploymentUpdate) 
                ? Task.FromResult<DeploymentUpdate?>(deploymentUpdate) 
                : Task.FromResult<DeploymentUpdate?>(null);
        }

        public Task<IEnumerable<DeploymentUpdate>> GetPendingDeployments()
        {
            var allQueues = PendingDeploymentUpdates.Values.ToList();
            var allPendingDeployments = allQueues.SelectMany(x => x.ToArray()).ToList();
            return Task.FromResult(allPendingDeployments.AsEnumerable());
        }
    }
}