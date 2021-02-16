using System.Collections.Generic;
using System.Threading.Tasks;
using Shipbot.ContainerRegistry.Models;
using Shipbot.Deployments.Models;

namespace Shipbot.Deployments
{
    /// <summary>
    ///     Dictates the flow of actions taken when a new deployment is triggered
    /// </summary>
    public interface IDeploymentWorkflowService
    {
        /// <summary>
        ///     Handles the logic required to deploy a new container image to one or more applications.
        ///     <remarks>
        ///         A new container image can be either detected by the repository polling jobs, OR through manual
        ///         submission from the Administrative REST API. 
        ///     </remarks> 
        /// </summary>
        /// <param name="latestImage"></param>
        /// <param name="isContainerRepositoryUpdate"></param>
        /// <returns>List of deployments that were created.</returns>
        Task<IEnumerable<Deployment>> StartImageDeployment(
            ContainerImage latestImage,
            bool isContainerRepositoryUpdate = false
        );

        Task<IEnumerable<Deployment>> StartImageDeployment(
            string applicationName,
            ContainerImage containerImage,
            bool isContainerRepositoryUpdate = false
            );

        Task<IEnumerable<Deployment>> StartImageDeployment(
            string containerImageRepository,
            IEnumerable<ContainerImage> newContainerImages,
            bool isContainerRepositoryUpdate = false
        );
    }
}