using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Deployments.GrainState;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Deployments.Grains
{
    public class DeploymentActionGrain : Grain<DeploymentActionState>, IDeploymentActionGrain
    {
        private readonly ILogger<DeploymentActionGrain> _log;
        private DeploymentActionKey _key;

        public DeploymentActionGrain(ILogger<DeploymentActionGrain> log)
        {
            _log = log;
        }
        
        public override async Task OnActivateAsync()
        {
            _key = (DeploymentActionKey) this.GetPrimaryKeyString();
            
            if (State.ApplicationEnvironmentKey == null)
            {
                State.ApplicationEnvironmentKey = new ApplicationEnvironmentKey(_key.Application, _key.Environment);
            }
            
            _log.LogInformation(
                $"Adding deployment action for '{{image}}' with tag '{{newTag}}' (from '{{currentTag}}') on '{{environment}}'",
                _key.ImageRepository,
                _key.TargetTag,
                State.CurrentTag ?? "",
                (string)State.ApplicationEnvironmentKey
            );

            // try
            // {
            //     if (State.Image == null)
            //     {
            //         var applicationEnvironmentGrain = GrainFactory.GetEnvironment(State.ApplicationEnvironmentKey);
            //
            //         var currentTags = await applicationEnvironmentGrain.GetCurrentImageTags();
            //         State.Image = currentTags.Keys.FirstOrDefault(x =>
            //             x.Repository == _key.ImageRepository && x.TagProperty.Path == _key.TagPropertyPath);
            //         State.CurrentTag = currentTags[State.Image];
            //         
            //         State.TargetTag = _key.TargetTag;
            //         
            //
            //     }
            // }
            // catch (Exception e)
            // {
            //     _log.LogError("abc", e);
            //     throw;
            // }
            
            await WriteStateAsync();

            await base.OnActivateAsync();
        }

        public Task SetParentDeploymentKey(DeploymentKey deploymentKey)
        {
            State.DeploymentKey = deploymentKey;
            return WriteStateAsync();
        }

        public Task Configure(Image image, string currentTag, string targetTag)
        {
            State.Image = image;
            State.CurrentTag = currentTag;
            State.TargetTag = targetTag;

            return WriteStateAsync();
        }

        public Task<Image> GetImage()
        {
            return Task.FromResult(State.Image);
        }

        public Task<string> GetTargetTag()
        {
            return Task.FromResult(State.TargetTag);
        }

        public Task<string> GetCurrentTag()
        {
            return Task.FromResult(State.CurrentTag);
        }

        public Task<DeploymentActionStatus> GetStatus()
        {
            return Task.FromResult(State.DeploymentActionStatus);
        }

        public Task SetStatus(DeploymentActionStatus status)
        {
            if (State.DeploymentActionStatus != status)
            {
                State.DeploymentActionStatus = status;
                return WriteStateAsync();
            }

            return Task.CompletedTask;
        }
    }
}