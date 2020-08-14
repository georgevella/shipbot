using System.Collections.Generic;
using System.Threading.Tasks;
using Shipbot.Models;

namespace Shipbot.Controller.Core.Deployments
{
    public interface IDeploymentService
    {
        Task AddDeploymentUpdate(Application application, Image image, string newTag);
        Task ChangeDeploymentUpdateStatus(DeploymentUpdate deploymentUpdate, DeploymentUpdateStatus status);

        Task FinishDeploymentUpdate(
            DeploymentUpdate deploymentUpdate,
            DeploymentUpdateStatus finalStatus
        );

        Task<IEnumerable<Models.DeploymentUpdate>> GetDeployments(Application application);
    }

    public interface IDeploymentQueueService
    {
        /// <summary>
        ///     Returns the next deployment update in the queue.
        /// </summary>
        /// <returns>Returns the next deployment update in the queue, or <c>null</c> if there are no pending deployment updates.</returns>
        Task<DeploymentUpdate?> GetNextPendingDeploymentUpdate(Application application);

        Task AddDeployment(Application application, DeploymentUpdate deploymentUpdate);
    }
}