using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Deployments.GrainKeys;
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
        public Task<(string Application, string ContainerRepository, string TargetTag)> GetDeploymentInformation() =>
            Task.FromResult(
                (State.Application!, State.ImageRepository!, State.TargetTag!)
            );

        public Task Configure(ApplicationKey key, ApplicationEnvironmentImageSettings image, string targetTag)
        {
            State.Application = key;
            State.ImageRepository = image.Repository;
            State.TargetTag = targetTag;
            
            return WriteStateAsync();
        }


        public async Task Deploy()
        {
            // TODO: handle multiple items in the deployment plan
            var deploymentActionKey = State.DeploymentActions[State.NextDeploymentActionIndex];
            var deploymentActionGrain = GrainFactory.GetDeploymentActionGrain(deploymentActionKey);
            
            var environmentGrain = GrainFactory.GetEnvironment(await deploymentActionGrain.GetApplicationEnvironment());
            var currentImageTagsInEnvironment = await environmentGrain.GetCurrentImageTags();
            //
            // prepare deployment action information
            
            
            // await deploymentActionGrain.Configure(
            //     deploymentPlan.Image, 
            //     currentImageTagsInEnvironment[deploymentPlan.Image], 
            //     deploymentPlan.TargetTag
            //     );
            //
            await deploymentActionGrain.SetStatus(
                DeploymentActionStatus.Pending 
                );

            // State.DeploymentActions.Add(deploymentActionKey);
            
            // get deployment source and start applying the deployment
            var deploymentSourceGrain =
                GrainFactory.GetHelmDeploymentSourceGrain(await deploymentActionGrain.GetApplicationEnvironment());

            // var environmentGrain = GrainFactory.GetEnvironment(
            //     new ApplicationEnvironmentKey(firstDeploymentAction.Application, firstDeploymentAction.Environment));
            // environmentGrain.GetCurrentImageTags();
            
            await deploymentSourceGrain.ApplyDeploymentAction(
                deploymentActionKey
            );
        }

        public async Task AddDeploymentAction(DeploymentAction deploymentAction)
        {
            var key = new DeploymentActionKey();

            State.DeploymentActions.Add(key);
            var deploymentActionGrain = GrainFactory.GetDeploymentActionGrain(key);
            await deploymentActionGrain.Configure(deploymentAction);
            await deploymentActionGrain.SetParentDeploymentKey(this.GetPrimaryKeyString());
            
            await  WriteStateAsync();
        }
        
        public Task<IEnumerable<DeploymentActionKey>> GetDeploymentActionIds()
        {
            return Task.FromResult(State.DeploymentActions.ToArray().AsEnumerable());
        }
    }
}