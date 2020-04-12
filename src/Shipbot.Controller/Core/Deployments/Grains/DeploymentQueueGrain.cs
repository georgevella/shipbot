using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Shipbot.Controller.Core.Deployments.Events;
using Shipbot.Controller.Core.Deployments.GrainKeys;
using Shipbot.Controller.Core.Deployments.GrainState;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.Core.DeploymentSources.Models;
using Shipbot.Controller.Core.Utilities;
using Shipbot.Controller.Core.Utilities.Eventing;

namespace Shipbot.Controller.Core.Deployments.Grains
{
    public class DeploymentQueueGrain : EventHandlingGrain<DeploymentQueue>, IDeploymentQueueGrain
    {
        private readonly ILogger<DeploymentQueueGrain> _log;
        private const string ReminderPrefix = "DeploymentQueueReminder";
        
        public DeploymentQueueGrain(ILogger<DeploymentQueueGrain> log)
        {
            _log = log;
        }

        public override async Task OnActivateAsync()
        {
            await SubscribeForEvents<DeploymentActionStatusChangeEvent>((change, token) =>
            {
                State.PendingDeploymentActions.Remove(change.ActionKey);

                return Task.CompletedTask;
            });

            for (var i = 1; i <= 6; i++)
            {
                var dueTime = (60 / 6) * i;
                await RegisterOrUpdateReminder($"{ReminderPrefix}_{dueTime}", TimeSpan.FromSeconds(dueTime),
                    TimeSpan.FromMinutes(1));
            }
            
            await base.OnActivateAsync();
        }
        
        public Task QueueDeploymentAction(DeploymentActionKey deploymentActionKey)
        {
            State.PendingDeploymentActions.Add(deploymentActionKey, new InternalDeploymentQueueItem(DateTime.Now, deploymentActionKey));
            return WriteStateAsync();
        }

        public async Task<IReadOnlyList<DeploymentQueueItem>> GetQueue()
        {
            var result = new List<DeploymentQueueItem>();
            foreach (var pair in State.PendingDeploymentActions)
            {
                var deploymentAction = GrainFactory.GetDeploymentActionGrain(pair.Key);

                var applicationEnvironment = await deploymentAction.GetApplicationEnvironment();

                result.Add(new DeploymentQueueItem(
                        pair.Value.QueuedOn,
                        pair.Value.Action,
                        applicationEnvironment.Application,
                        applicationEnvironment.Environment,
                        await deploymentAction.GetCurrentTag(),
                        await deploymentAction.GetTargetTag(),
                        await deploymentAction.GetStatus()
                    )
                );
            }

            return result.AsReadOnly();
        }


        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            using (_log.BeginShipbotLogScope())
            {
                var deploymentActionKeys = new List<InternalDeploymentQueueItem>(State.PendingDeploymentActions.Values.ToList());
                State.PendingDeploymentActions.Clear();
                await WriteStateAsync();

                foreach (var item in deploymentActionKeys)
                {
                    var deploymentActionGrain = GrainFactory.GetDeploymentActionGrain(item.Action);
                
                    await deploymentActionGrain.SetStatus(
                        DeploymentActionStatus.Pending 
                    );

                    // get deployment source and start applying the deployment
                    var deploymentSourceGrain = GrainFactory
                        .GetHelmDeploymentSourceGrain(await deploymentActionGrain.GetApplicationEnvironment());
                    
                    // build change action from deployment action
                    var imageSettings = await deploymentActionGrain.GetImage();
                    var currentTag = await deploymentActionGrain.GetCurrentTag();
                    var targetTag = await deploymentActionGrain.GetTargetTag();
                    var deploymentSourceChangeAction = new DeploymentSourceChange(
                        DeploymentSourceChangeAction.Replace,
                        imageSettings.ImageTagValuePath,
                        currentTag,
                        targetTag
                    );

                    await deploymentSourceGrain.ApplyDeploymentAction(
                        deploymentSourceChangeAction
                    );
                    
                    // TODO: requeue events if change failed
                    await deploymentActionGrain.SetStatus(DeploymentActionStatus.Complete);
                }    
            }
        }
    }

    public interface IDeploymentQueueGrain : IGrainWithStringKey, IRemindable
    {
        Task QueueDeploymentAction(DeploymentActionKey deploymentActionKey);

        Task<IReadOnlyList<DeploymentQueueItem>> GetQueue();
    }
}