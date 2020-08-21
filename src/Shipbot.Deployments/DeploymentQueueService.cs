using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shipbot.Models;

namespace Shipbot.Controller.Core.Deployments
{
    public class DeploymentQueueService : IDeploymentQueueService
    {
        private readonly object _lock = new object();
        private readonly ConcurrentDictionary<Application, ConcurrentQueue<DeploymentUpdate>> _pendingDeploymentUpdates = new ConcurrentDictionary<Application, ConcurrentQueue<DeploymentUpdate>>();

        public Task AddDeployment(Application application, DeploymentUpdate deploymentUpdate)
        {
            lock (_lock)
            {
                var queue = _pendingDeploymentUpdates.GetOrAdd(application,
                    key => new ConcurrentQueue<DeploymentUpdate>());
                queue.Enqueue(deploymentUpdate);

                return Task.CompletedTask;
            }
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

        public Task<IEnumerable<DeploymentUpdate>> GetPendingDeployments()
        {
            var allQueues = _pendingDeploymentUpdates.Values.ToList();
            var allPendingDeployments = allQueues.SelectMany(x => x.ToArray()).ToList();
            return Task.FromResult(allPendingDeployments.AsEnumerable());
        }
    }
}