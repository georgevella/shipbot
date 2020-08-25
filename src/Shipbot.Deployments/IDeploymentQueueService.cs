using System.Collections.Generic;
using System.Threading.Tasks;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Models;

namespace Shipbot.Controller.Core.Deployments
{
    public interface IDeploymentQueueService
    {
        /// <summary>
        ///     Returns the next deployment update in the queue.
        /// </summary>
        /// <returns>Returns the next deployment update in the queue, or <c>null</c> if there are no pending deployment updates.</returns>
        Task<DeploymentUpdate?> GetNextPendingDeploymentUpdate(Application application);

        Task AddDeployment(Deployment deployment);
        Task<IEnumerable<DeploymentUpdate>> GetPendingDeployments();
    }
}