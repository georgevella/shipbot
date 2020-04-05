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
        public override async Task OnActivateAsync()
        {
            var key = (DeploymentKey) this.GetPrimaryKeyString();
            State.Application = key.Application;
            State.ImageRepository = key.ImageRepository;
            State.TargetTag = key.TargetTag;

            await WriteStateAsync();
            
            await base.OnActivateAsync();
        }

        public async Task Deploy()
        {
            // TODO: handle multiple items in the deployment plan
            var deploymentPlan = State.DeploymentPlan.First();
            
            var applicationEnvironmentKey = new ApplicationEnvironmentKey(
                deploymentPlan.Application, 
                deploymentPlan.Environment
                );
            // var environmentGrain = GrainFactory.GetEnvironment(applicationEnvironmentKey);
            // var currentImageTagsInEnvironment = await environmentGrain.GetCurrentImageTags();
            //
            // prepare deployment action information
            var deploymentActionKey = new DeploymentActionKey(
                deploymentPlan.Application,
                deploymentPlan.Environment, 
                deploymentPlan.Image.Repository, 
                deploymentPlan.Image.TagProperty.Path,
                deploymentPlan.TargetTag
            );

            State.DeploymentActions.Add(deploymentActionKey);
            var deploymentActionGrain = GrainFactory.GetDeploymentActionGrain(deploymentActionKey);
            // await deploymentActionGrain.Configure(
            //     deploymentPlan.Image, 
            //     currentImageTagsInEnvironment[deploymentPlan.Image], 
            //     deploymentPlan.TargetTag
            //     );
            //
            await deploymentActionGrain.SetStatus(
                DeploymentActionStatus.Pending 
                );
            await deploymentActionGrain.SetParentDeploymentKey(this.GetPrimaryKeyString());

            State.DeploymentActions.Add(deploymentActionKey);
            
            // get deployment source and start applying the deployment
            var deploymentSourceGrain = GrainFactory.GetHelmDeploymentSourceGrain(new ApplicationEnvironmentKey(deploymentActionKey.Application, deploymentActionKey.Environment));

            // var environmentGrain = GrainFactory.GetEnvironment(
            //     new ApplicationEnvironmentKey(firstDeploymentAction.Application, firstDeploymentAction.Environment));
            // environmentGrain.GetCurrentImageTags();
            
            await deploymentSourceGrain.ApplyDeploymentAction(
                deploymentActionKey
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