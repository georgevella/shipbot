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
using Shipbot.Controller.Core.Apps.GrainState;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Deployments.Events;
using Shipbot.Controller.Core.Deployments.GrainKeys;
using Shipbot.Controller.Core.Deployments.GrainState;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.Core.Models;
using Shipbot.Controller.Core.Utilities;
using Shipbot.Controller.Core.Utilities.Eventing;

namespace Shipbot.Controller.Core.Deployments.Grains
{
    /// <summary>
    ///     Describes an image deployment for an application.  
    /// </summary>
    [StorageProvider(ProviderName = ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)]
    [ImplicitStreamSubscription(DeploymentStreamingConstants.DeploymentsNamespace)]
    public class DeploymentGrain : EventHandlingGrain<DeploymentState>, IDeploymentGrain
    {
        private readonly ILogger<DeploymentGrain> _log;

        public DeploymentGrain(ILogger<DeploymentGrain> log)
        {
            _log = log;
        }
        
        public override async Task OnActivateAsync()
        {
            await SubscribeForEvents<DeploymentActionStatusChangeEvent>((change, token) =>
            {
                // do we know about this deployment action
                if (!State.DeploymentActions.Contains(change.ActionKey))
                    return Task.CompletedTask;
                
                _log.Info("DeploymentAction {deploymentActionKey} changed state {fromStatus}->{toStatus}",
                    change.ActionKey,
                    change.FromStatus,
                    change.ToStatus
                );
                
                return Task.CompletedTask;
            });

            var deploymentKey = new DeploymentKey(this.GetPrimaryKey());

            await SubscribeToPrivateMessaging<NewDeploymentEvent>(deploymentKey.Id,
                DeploymentStreamingConstants.DeploymentsNamespace,
                (e, token) =>
                {
                    return Configure(e.ApplicationEnvironment, e.Image, e.TargetTag);
                });
            
            await base.OnActivateAsync();
        }

        public Task<(string Application, string ContainerRepository, string TargetTag, DeploymentStatus Status)> GetDeploymentInformation() =>
            Task.FromResult(
                (State.Application!.Name, State.ImageRepository!, State.TargetTag!, State.Status)
            );

        public async Task Configure(
            ApplicationEnvironmentKey firstApplicationEnvironment, 
            ApplicationEnvironmentImageMetadata image, 
            string targetTag)
        {
        
            using var logScope = _log.BeginShipbotLogScope(firstApplicationEnvironment);
            
            State.Application = firstApplicationEnvironment.Application;
            State.ImageRepository = image.Repository;
            State.TargetTag = targetTag;
            
            var targetEnvironments = new List<ApplicationEnvironmentKey>
            {
                firstApplicationEnvironment
            };
        
            // determine if we have other environments we want to promote to.
            var firstEnvironmentGrain = GrainFactory.GetEnvironment(firstApplicationEnvironment);
            var promotionSettings = await firstEnvironmentGrain.GetDeploymentPromotionSettings();
             targetEnvironments.AddRange(
                 promotionSettings.Select(
                     promoteToEnv => new ApplicationEnvironmentKey(
                         State.Application,
                         promoteToEnv
                     )
                 )
             );
            
            foreach (var env in targetEnvironments) {
                var envGrain = GrainFactory.GetEnvironment(env);
                var currentTag = await envGrain.GetImageTag(image);

                // build deployment action metadata
                var deploymentAction = new DeploymentAction()
                {
                    Image = image,
                    ApplicationEnvironmentKey = env,
                    CurrentTag = currentTag,
                    TargetTag = targetTag
                };
                
                // create deployment key
                var key = new DeploymentActionKey(Guid.NewGuid());
                State.DeploymentActions.Add(key);
                
                // configure deployment action
                var deploymentActionGrain = GrainFactory.GetDeploymentActionGrain(key);
                await deploymentActionGrain.Configure(deploymentAction);
                await deploymentActionGrain.SetParentDeploymentKey(this.GetPrimaryKeyString());
            }
            
            await WriteStateAsync();
        }
        
        public async Task SubmitNextDeploymentAction()
        {
            // TODO: handle multiple items in the deployment plan
            using (_log.BeginShipbotLogScope(State.Application))
            {
                var deploymentActionKey = State.DeploymentActions[State.NextDeploymentActionIndex];

                // move pointer to next deployment action
                State.NextDeploymentActionIndex++;

                var deploymentQueue = GrainFactory.GetDeploymentQueueGrain();
                await deploymentQueue.QueueDeploymentAction(deploymentActionKey);
                
                await WriteStateAsync();
            }
        }

        public Task<IEnumerable<DeploymentActionKey>> GetDeploymentActionIds()
        {
            return Task.FromResult(State.DeploymentActions.ToArray().AsEnumerable());
        }
    }
    
    public interface IDeploymentGrain : IGrainWithGuidKey
    {
        Task<IEnumerable<DeploymentActionKey>> GetDeploymentActionIds();
        Task SubmitNextDeploymentAction();
        Task<(string Application, string ContainerRepository, string TargetTag, DeploymentStatus Status)> GetDeploymentInformation();
        // Task Configure(ApplicationEnvironmentKey firstApplicationEnvironment, ApplicationEnvironmentImageMetadata image, string targetTag);
    }
}