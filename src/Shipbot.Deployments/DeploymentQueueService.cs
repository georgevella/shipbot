using System.Collections.Concurrent;
using System.Threading.Tasks;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Deployments
{
    public class DeploymentQueueService : IDeploymentQueueService
    {
        private readonly ConcurrentDictionary<Application, ConcurrentQueue<DeploymentUpdate>> _pendingDeploymentUpdates = new ConcurrentDictionary<Application, ConcurrentQueue<DeploymentUpdate>>();

        public Task AddDeployment(Application application, DeploymentUpdate deploymentUpdate)
        {
            var queue = _pendingDeploymentUpdates.GetOrAdd(application, key => new ConcurrentQueue<DeploymentUpdate>());
            queue.Enqueue( deploymentUpdate );
            
            return Task.CompletedTask;
        }
        
        public Task<DeploymentUpdate?> GetNextPendingDeploymentUpdate(Application application)
        {
            // are there any pending deployments
            if (!_pendingDeploymentUpdates.TryGetValue(application, out var queue))
                return Task.FromResult<DeploymentUpdate?>(null);
            
            return queue.TryDequeue(out var deploymentUpdate) 
                ? Task.FromResult<DeploymentUpdate?>(deploymentUpdate) 
                : Task.FromResult<DeploymentUpdate?>(null);
        }
    }
}