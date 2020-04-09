using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Orleans;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Deployments.GrainKeys;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Deployments.Grains
{
    public interface IDeploymentGrain : IGrainWithStringKey
    {
        // Task AddDeploymentUpdate(DeploymentUpdate deploymentUpdate);
        // 
        // Task ChangeDeploymentUpdateStatus(DeploymentUpdate deploymentUpdate, DeploymentUpdateStatus status);
        // Task FinishDeploymentUpdate(DeploymentUpdate deploymentUpdate);
        //
        // Task FailDeploymentUpdate(DeploymentUpdate deploymentUpdate);
        
        Task<IEnumerable<DeploymentActionKey>> GetDeploymentActionIds();
        Task Deploy();
        Task AddDeploymentAction(DeploymentAction deploymentAction);
        Task<(string Application, string ContainerRepository, string TargetTag)> GetDeploymentInformation();
        Task Configure(ApplicationKey key, ApplicationEnvironmentImageSettings image, string targetTag);
    }
}