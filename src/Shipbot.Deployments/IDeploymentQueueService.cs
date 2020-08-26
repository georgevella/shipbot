using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shipbot.Deployments.Models;
using Shipbot.Models;

namespace Shipbot.Deployments
{
    public interface IDeploymentQueueService
    {
        /// <summary>
        ///     Returns the next deployment update in the queue.
        /// </summary>
        /// <returns>Returns the next deployment update in the queue, or <c>null</c> if there are no pending deployment updates.</returns>
        Task<Deployment?> GetNextPendingDeploymentUpdate(Application application);

        Task EnqueueDeployment(
            Deployment deployment,
            TimeSpan? delay = null 
            );
        Task<IEnumerable<Deployment>> GetPendingDeployments();
    }
}