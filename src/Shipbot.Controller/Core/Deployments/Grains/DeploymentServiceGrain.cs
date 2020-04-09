using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Deployments.GrainState;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.Core.Utilities;

namespace Shipbot.Controller.Core.Deployments.Grains
{
    [StorageProvider(ProviderName = ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)]
    public class DeploymentServiceGrain : Grain<DeploymentServiceState>, IDeploymentServiceGrain
    {
        private readonly ILogger<DeploymentServiceGrain> _log;
        private ApplicationKey _key;

        public DeploymentServiceGrain(
            ILogger<DeploymentServiceGrain> log
            )
        {
            _log = log;
        }

        public override Task OnActivateAsync()
        {
            _key = (ApplicationKey) this.GetPrimaryKeyString();
            
            return base.OnActivateAsync();
        }

        public async Task<DeploymentKey> CreateNewImageDeployment(
            string environment, 
            ApplicationEnvironmentImageSettings image, 
            string newTag
            )
        {
            using (_log.BeginShipbotLogScope(_key, environment))
            {
                var firstApplicationEnvironmentKey = new ApplicationEnvironmentKey(_key, environment);
                
                // start planning the first environment we are deploying to
                var deploymentPlan = new List<DeploymentAction>();
                var targetEnvironments = new List<string>
                {
                    environment
                };

                // determine if we have other environments we want to promote to.
                var firstEnvironmentGrain = GrainFactory.GetEnvironment(firstApplicationEnvironmentKey);
                var promotionSettings = await firstEnvironmentGrain.GetDeploymentPromotionSettings();
                targetEnvironments.AddRange(promotionSettings);

                // generate image deployment (starting with the identifier)
                var deploymentKey = new DeploymentKey();
                
                // build deployment plan
                foreach (var env in targetEnvironments)
                {
                    var applicationEnvironmentKey = new ApplicationEnvironmentKey(_key, env);
                    var environmentGrain = GrainFactory.GetEnvironment(applicationEnvironmentKey);

//                    var imageUpdatePolicy = await environmentGrain.GetImageUpdatePolicy(image);
                    var currentTags = await environmentGrain.GetCurrentImageTags();
                    
                    var plannedDeploymentAction = new DeploymentAction()
                    {
                        Image = image,
                        ApplicationEnvironmentKey = applicationEnvironmentKey,
                        CurrentTag = currentTags[image],
                        TargetTag = newTag
                    };
                    
                    // check if there is already an planned deployment action for this image
                    if (State.PlannedDeploymentActionsIndex.ContainsKey(plannedDeploymentAction))
                    {
                        var otherDeploymentKey = State.PlannedDeploymentActionsIndex[plannedDeploymentAction];
                        _log.LogWarning(
                            "A deployment action for {image} with tag {newTag} is already planned and will be executed by ({deploymentKey}), skipping ...",
                            image.Repository,
                            newTag,
                            otherDeploymentKey
                        );

                        // TODO: maybe this needs to be handled better?
                        return null;
                    }

                    State.PlannedDeploymentActionsIndex[plannedDeploymentAction] = deploymentKey;
                    deploymentPlan.Add(plannedDeploymentAction);

                    // // check if we have a deployment for the same image
                    // if (State.DeploymentActions.Contains(deploymentUpdateKey))
                    // {
                    //     _log.LogWarning(
                    //         "A deployment action for {image} with tag {newTag} already present ({deploymentActionKey}), skipping ...",
                    //         image.Repository, 
                    //         newTag, 
                    //         deploymentUpdateKey
                    //         );
                    //     return null;
                    // }
                    //
                    // State.DeploymentActions.Add(deploymentUpdateKey);
                    //
                    // // check if there is a pending deployment with a newer image.
                    // var applicationImageKey = new ApplicationImageKey(applicationEnvironmentKey, image);
                    // // TODO move this to auto  deploy logic space (before calling into DeploymentService)
                    // if (State.LatestImageDeployments.TryGetValue(applicationImageKey, out var latestImageDeploymentKey))
                    // {
                    //     if (!imageUpdatePolicy.IsGreaterThen(newTag, latestImageDeploymentKey.TargetTag))
                    //     {
                    //         _log.LogWarning(
                    //             "A newer tag ({currentTag}) for {image} is already deployed or pending deployment, skipping ({newTag})",
                    //             latestImageDeploymentKey.TargetTag,
                    //             image.Repository,
                    //             newTag
                    //         );
                    //         return null;
                    //     }
                    // }       
                    //
                    // State.LatestImageDeployments[applicationImageKey] = deploymentUpdateKey;
                    //
                    // // start building deployment update grain
                    // var deploymentUpdateGrain = GrainFactory.GetDeploymentActionGrain(deploymentUpdateKey);
                    // await deploymentUpdateGrain.Configure(image, currentTags[image], newTag);
                    // // await deploymentUpdateGrain.SetStatus(
                    // //     targetEnvironments.IndexOf(env) == 0 ? DeploymentUpdateStatus.Pending : DeploymentUpdateStatus.Created 
                    // //     );
                    // await deploymentUpdateGrain.SetParentDeploymentKey(deploymentKey);
                    //
                    // deploymentActionKeys.Add(deploymentUpdateKey);
                }

                if (!deploymentPlan.Any())
                {
                    return null;
                }
                
                // start building deployment
                var deploymentGrain = GrainFactory.GetDeploymentGrain(deploymentKey);
                await deploymentGrain.Configure(_key, image, newTag);
                deploymentPlan.ForEach( async x=> await deploymentGrain.AddDeploymentAction(x) );
                
                //await deploymentGrain.Configure(_key, image, newTag);
                State.Deployments.Add(deploymentKey);
                    
                await WriteStateAsync();
                return deploymentKey;

                

                // check if the new tag is newer than the current tag.
                // NOTE: the chunk below is already being handled by ContainerRegistryStreamObserver
//            if (!image.Policy.IsGreaterThen(newTag, currentTag))
//            {
//                _log.LogWarning(
//                    "A newer tag ({currentTag}) for {image} is already deployed to '{application}:{environment}', skipping '{newTag}'",
//                    currentTag,
//                    image.Repository,
//                    applicationEnvironmentKey.Application,
//                    applicationEnvironmentKey.Environment,
//                    newTag
//                );
//                return;
//            }

                
                
                // register deployment in index
            }

        }

        public Task<IEnumerable<DeploymentKey>> GetAllDeploymentIds()
        {
            return Task.FromResult(State.Deployments.ToList().AsEnumerable());
        }
    }
}