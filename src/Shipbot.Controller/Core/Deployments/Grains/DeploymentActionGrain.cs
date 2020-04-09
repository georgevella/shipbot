using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Deployments.GrainKeys;
using Shipbot.Controller.Core.Deployments.GrainState;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Deployments.Grains
{
    public class DeploymentActionGrain : Grain<DeploymentActionState>, IDeploymentActionGrain
    {
        private readonly ILogger<DeploymentActionGrain> _log;
        public DeploymentActionGrain(ILogger<DeploymentActionGrain> log)
        {
            _log = log;
        }
        
        public Task SetParentDeploymentKey(DeploymentKey deploymentKey)
        {
            State.DeploymentKey = deploymentKey;
            return WriteStateAsync();
        }

        public Task<ApplicationEnvironmentImageSettings> GetImage() => Task.FromResult(State.Action.Image);

        public Task<string> GetTargetTag() => Task.FromResult(State.Action.TargetTag);

        public Task<string> GetCurrentTag() =>Task.FromResult<string>(
            State.Action.CurrentTag);

        public Task<ApplicationEnvironmentKey> GetApplicationEnvironment() =>
            Task.FromResult(State.Action.ApplicationEnvironmentKey);

        public Task<DeploymentAction> GetAction()
        {
            return Task.FromResult(State.Action);
        }

        public Task Configure(DeploymentAction deploymentAction)
        {
            if (State.Action == null)
            {
                _log.LogInformation(
                    $"Adding deployment action for '{{image}}' with tag '{{newTag}}' (from '{{currentTag}}') on '{{environment}}'",
                    deploymentAction.Image.Repository,
                    deploymentAction.TargetTag,
                    deploymentAction.CurrentTag ?? "",
                    (string)deploymentAction.ApplicationEnvironmentKey
                );
                
                State.Action = deploymentAction;
            }
            else
            {
                throw new InvalidOperationException("DeploymentAction already setup.");
            }
            
            return WriteStateAsync();
        }

        public Task<DeploymentActionStatus> GetStatus()
        {
            return Task.FromResult(State.Action.Status);
        }

        public Task SetStatus(DeploymentActionStatus status)
        {
            if (State.Action.Status == status) 
                return Task.CompletedTask;
            
            State.Action.Status = status;
            return WriteStateAsync();

        }
    }
}