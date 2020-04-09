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
                        
                        return DeploymentKey.Empty;
                    }
                    
                    deploymentPlan.Add(plannedDeploymentAction);
                }

                if (!deploymentPlan.Any())
                {
                    return DeploymentKey.Empty;
                }

                // start building deployment
                var deploymentGrain = GrainFactory.GetDeploymentGrain(deploymentKey);
                await deploymentGrain.Configure(_key, image, newTag);
                deploymentPlan.ForEach( async x=>
                {
                    // store reference to the deployment that will execute this action
                    State.PlannedDeploymentActionsIndex[x] = deploymentKey;
                    await deploymentGrain.AddDeploymentAction(x);
                });
                
                //await deploymentGrain.Configure(_key, image, newTag);
                State.Deployments.Add(deploymentKey);
                    
                await WriteStateAsync();
                return deploymentKey;
            }

        }

        public Task<IEnumerable<DeploymentKey>> GetAllDeploymentIds()
        {
            return Task.FromResult(State.Deployments.ToList().AsEnumerable());
        }
    }
}