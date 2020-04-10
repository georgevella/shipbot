using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Deployments.Events;
using Shipbot.Controller.Core.Deployments.GrainKeys;
using Shipbot.Controller.Core.Deployments.GrainState;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.Core.Models;
using Shipbot.Controller.Core.Utilities.Eventing;

namespace Shipbot.Controller.Core.Deployments.Grains
{
    /// <summary>
    ///     Describes an image deployment for an application.  
    /// </summary>
    [StorageProvider(ProviderName = ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)]
    public class DeploymentGrain : EventHandlingGrain<DeploymentState>, IDeploymentGrain
    {
        private readonly ILogger<DeploymentGrain> _log;

        public DeploymentGrain(ILogger<DeploymentGrain> log)
        {
            _log = log;
        }
        public override async Task OnActivateAsync()
        {
            await SubscribeForEvents<DeploymentActionStatusChange>((change, token) =>
            {
                _log.Info("DeploymentAction {deploymentActionKey} changed state {fromStatus}->{toStatus}",
                    change.ActionKey,
                    change.FromStatus,
                    change.ToStatus
                );
                
                return Task.CompletedTask;
            });
            
            await base.OnActivateAsync();
        }

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

            await deploymentActionGrain.SetStatus(
                DeploymentActionStatus.Pending 
                );

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
            var key = new DeploymentActionKey(Guid.NewGuid());

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