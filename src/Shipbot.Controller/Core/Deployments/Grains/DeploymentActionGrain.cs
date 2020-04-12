using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Shipbot.Controller.Core.Apps.GrainState;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Deployments.Events;
using Shipbot.Controller.Core.Deployments.GrainKeys;
using Shipbot.Controller.Core.Deployments.GrainState;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.Core.Models;
using Shipbot.Controller.Core.Utilities.Eventing;

namespace Shipbot.Controller.Core.Deployments.Grains
{
    public class DeploymentActionGrain : EventHandlingGrain<DeploymentActionState>, IDeploymentActionGrain
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

        public Task<ApplicationEnvironmentImageKey> GetImage() => Task.FromResult(State.Action.Image);

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

        public async Task SetStatus(DeploymentActionStatus status)
        {
            if (State.Action.Status == status) 
                return;

            var currentStatus = State.Action.Status;
            State.Action.Status = status;
            await WriteStateAsync();
            await SendEvent(new DeploymentActionStatusChangeEvent(this.GetPrimaryKeyString(), currentStatus, status));
        }
    }
    
    public interface IDeploymentActionGrain : IGrainWithStringKey
    {
        Task<DeploymentActionStatus> GetStatus();

        Task SetStatus(DeploymentActionStatus status);


        Task SetParentDeploymentKey(DeploymentKey deploymentKey);
        Task<ApplicationEnvironmentImageKey> GetImage();
        Task<string> GetTargetTag();
        Task<string> GetCurrentTag();
        Task<DeploymentAction> GetAction();
        Task Configure(DeploymentAction deploymentAction);
        Task<ApplicationEnvironmentKey> GetApplicationEnvironment();
    }
}