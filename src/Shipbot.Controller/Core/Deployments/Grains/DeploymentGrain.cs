using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Deployments.GrainState;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Deployments.Grains
{
    /// <summary>
    ///     Describes an image deployment for an application.  
    /// </summary>
    [StorageProvider(ProviderName = ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)]
    public class DeploymentGrain : Grain<DeploymentState>, IDeploymentGrain
    {
        public Task Configure(ApplicationKey application, Image image, string tag)
        {
            State.Application = application;
            State.ImageRepository = image.Repository;
            State.TargetTag = tag;

            return WriteStateAsync();
        }

        public Task Deploy()
        {
            var firstDeploymentAction = State.DeploymentActions.First();

            var deploymentSourceGrain = GrainFactory.GetHelmDeploymentSourceGrain(new ApplicationEnvironmentKey(firstDeploymentAction.Application, firstDeploymentAction.Environment));

            // var environmentGrain = GrainFactory.GetEnvironment(
            //     new ApplicationEnvironmentKey(firstDeploymentAction.Application, firstDeploymentAction.Environment));
            // environmentGrain.GetCurrentImageTags();
            
            return deploymentSourceGrain.ApplyDeploymentAction(
                firstDeploymentAction
            );
        }

        public Task AddDeploymentPlanAction(PlannedDeploymentAction plannedDeploymentAction)
        {
            State.DeploymentPlan.Add(plannedDeploymentAction);
            return WriteStateAsync();
        }

        public Task AddDeploymentActionId(DeploymentActionKey deploymentActionKey)
        {
            State.DeploymentActions.Add(deploymentActionKey);
            return WriteStateAsync();
        }

        public Task<IEnumerable<DeploymentActionKey>> GetDeploymentActionIds()
        {
            return Task.FromResult(State.DeploymentActions.ToArray().AsEnumerable());
        }

        public Task<IEnumerable<PlannedDeploymentAction>> GetDeploymentPlan()
        {
            return Task.FromResult(State.DeploymentPlan.ToArray().AsEnumerable());
        }
    }
}