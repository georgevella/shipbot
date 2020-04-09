using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.ContainerRegistry.Models;
using Shipbot.Controller.Core.ContainerRegistry.Watcher;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Deployments.Grains
{
    public interface IDeploymentServiceGrain : IGrainWithStringKey
    {
        Task<DeploymentKey> CreateNewImageDeployment(string environment, ApplicationEnvironmentImageSettings image, string newTag);
        
        //Task ChangeDeploymentUpdateStatus(DeploymentUpdate deploymentUpdate, DeploymentUpdateStatus status);

//        /// <summary>
//        ///     Returns the next deployment update in the queue.
//        /// </summary>
//        /// <returns>Returns the next deployment update in the queue, or <c>null</c> if there are no pending deployment updates.</returns>
//        Task<DeploymentUpdate> GetNextPendingDeploymentUpdate(ApplicationEnvironmentKey applicationEnvironmentKey);
//
//        Task FinishDeploymentUpdate(
//            DeploymentUpdate deploymentUpdate,
//            DeploymentUpdateStatus finalStatus
//        );
//
//        Task PromoteDeployment(DeploymentUpdate deploymentUpdate);
//
//        Task PromoteDeployment(Application application, string containerRepository, string targetTag,
//            string sourceEnvironment);
        Task<IEnumerable<DeploymentKey>>  GetAllDeploymentIds();
    }
}