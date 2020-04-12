using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streams;
using Shipbot.Controller.Core.Apps.GrainState;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Deployments.Events;
using Shipbot.Controller.Core.Deployments.GrainState;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.Core.Utilities;
using Shipbot.Controller.Core.Utilities.Eventing;

namespace Shipbot.Controller.Core.Deployments.Grains
{
    [StorageProvider(ProviderName = ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)]
    public class DeploymentServiceGrain : EventHandlingGrain<DeploymentServiceState>, IDeploymentServiceGrain
    {
        private readonly ILogger<DeploymentServiceGrain> _log;

        public DeploymentServiceGrain(
            ILogger<DeploymentServiceGrain> log
            )
        {
            _log = log;
        }

        public override async Task OnActivateAsync()
        {
            await SubscribeForEvents<DeploymentStatusChangeEvent>((change, token) =>
            {
                _log.Info("Deployment {deploymentKey} changed state {fromStatus}->{toStatus}", 
                    change.DeploymentKey, 
                    change.FromStatus,
                    change.ToStatus
                );
                
                return Task.CompletedTask;
            });

            await SubscribeForEvents<NewDeploymentEvent>(async (e, token) =>
            {
                using (_log.BeginShipbotLogScope(e.ApplicationEnvironment))
                {
                    if (e.ApplicationEnvironment.Application != this.GetPrimaryKeyString())
                        return;

                    var firstApplicationEnvironmentKey = e.ApplicationEnvironment;
                    
                    // start planning the first environment we are deploying to
                    //var deploymentPlan = new List<DeploymentAction>();
                    var targetEnvironments = new List<string>
                    {
                        e.ApplicationEnvironment.Environment
                    };

                    var deploymentKey = new DeploymentKey(Guid.NewGuid());
                    var plannedActions =
                        new List<(
                            string environment, 
                            string imageRepository, 
                            string imageTagValuePath, 
                            string targetTag
                            )>();
                    // determine if we have other environments we want to promote to.
                    {
                        var firstEnvironmentGrain = GrainFactory.GetEnvironment(firstApplicationEnvironmentKey);
                        var promotionSettings = await firstEnvironmentGrain.GetDeploymentPromotionSettings();
                        targetEnvironments.AddRange(promotionSettings);

                        // generate image deployment (starting with the identifier)
                        
                        var currentTag = await firstEnvironmentGrain.GetImageTag(e.Image);

                        // build deployment plan
                        foreach (var env in targetEnvironments)
                        {
                            var plannedDeploymentAction = (
                                env,
                                e.Image.Repository,
                                e.Image.ImageTagValuePath,
                                e.TargetTag
                            );

                            // check if there is already an planned deployment action for this image
                            if (State.PlannedDeploymentActionsIndex.ContainsKey(plannedDeploymentAction))
                            {
                                var otherDeploymentKey = State.PlannedDeploymentActionsIndex[plannedDeploymentAction];
                                _log.LogWarning(
                                    "A deployment action for {image} with tag {targetTag} is already planned and will be executed by ({deploymentKey}), skipping ...",
                                    e.Image.Repository,
                                    e.TargetTag,
                                    otherDeploymentKey
                                );

                                return;
                            }
                            plannedActions.Add(plannedDeploymentAction);
                        }
                    }

                    // start building deployment
                    _log.Trace("Sending new deployment event to {deploymentKey}", deploymentKey);
                    await SendMessage(deploymentKey.Id, e, DeploymentStreamingConstants.DeploymentsNamespace);
                    
                    // store reference to deployment
                    State.Deployments.Add(deploymentKey);
                    plannedActions.ForEach( x => State.PlannedDeploymentActionsIndex[x] = deploymentKey);
                    await WriteStateAsync();
                }
            });

            await base.OnActivateAsync();
        }


        public Task Hello()
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<DeploymentKey>> GetAllDeploymentIds()
        {
            return Task.FromResult(State.Deployments.ToList().AsEnumerable());
        }
    }
    
    public interface IDeploymentServiceGrain : IGrainWithStringKey
    {
        //Task<DeploymentKey> CreateNewImageDeployment(string environment, ApplicationEnvironmentImageMetadata image, string targetTag);

        Task Hello();
        
        Task<IEnumerable<DeploymentKey>>  GetAllDeploymentIds();
    }
}