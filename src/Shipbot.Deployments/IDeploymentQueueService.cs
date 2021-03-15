using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shipbot.Applications.Models;
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

        /// <summary>
        ///     Add a deployment to the queue.
        /// </summary>
        /// <param name="deployment"></param>
        /// <param name="delay"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        Task<bool> EnqueueDeployment(
            Deployment deployment,
            TimeSpan? delay = null,
            bool force = false
            );
        Task<IEnumerable<Deployment>> GetPendingDeployments();
    }
}