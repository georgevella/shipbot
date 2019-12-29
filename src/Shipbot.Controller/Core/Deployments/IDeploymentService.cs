//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Shipbot.Controller.Core.Apps.Models;
//using Shipbot.Controller.Core.Deployments.Models;
//using Shipbot.Controller.Core.Models;
//
//namespace Shipbot.Controller.Core.Deployments
//{
//    public interface IDeploymentService
//    {
//        Task AddDeploymentUpdate(string containerRepository, IEnumerable<ImageTag> tags);
//
//        Task AddDeploymentUpdate(Application application, ApplicationEnvironment environment, Image image,
//            string newTag);
//        
//        Task ChangeDeploymentUpdateStatus(DeploymentUpdate deploymentUpdate, DeploymentUpdateStatus status);
//
//        /// <summary>
//        ///     Returns the next deployment update in the queue.
//        /// </summary>
//        /// <returns>Returns the next deployment update in the queue, or <c>null</c> if there are no pending deployment updates.</returns>
//        Task<DeploymentUpdate> GetNextPendingDeploymentUpdate(Application application, ApplicationEnvironment environment);
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
//    }
//}