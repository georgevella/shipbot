using System;
using System.Collections.Generic;
using System.Linq;
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
using Shipbot.Controller.Core.Slack;
using Shipbot.Controller.Core.Utilities;
using Shipbot.Controller.Core.Utilities.Eventing;
using SlackAPI;

namespace Shipbot.Controller.Core.Deployments.Grains
{
    /// <summary>
    ///     Describes an image deployment for an application.  Instances of this grain are activated by the streaming
    ///     processor. 
    /// </summary>
    [StorageProvider(ProviderName = ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)]
    [ImplicitStreamSubscription(DeploymentStreamingConstants.DeploymentsNamespace)]
    public class DeploymentGrain : EventHandlingGrain<DeploymentState>, IDeploymentGrain
    {
        private readonly ILogger<DeploymentGrain> _log;
        private readonly ISlackClient _slackClient;

        public DeploymentGrain(
            ILogger<DeploymentGrain> log,
            ISlackClient slackClient
            )
        {
            _log = log;
            _slackClient = slackClient;
        }
        
        public override async Task OnActivateAsync()
        {   
            // subscribe for deployment action status changes
            await SubscribeForEvents<DeploymentActionStatusChangeEvent>(async (change, token) =>
            {
                // do we know about this deployment action
                if (!State.DeploymentActions.Contains(change.ActionKey, DeploymentActionKey.EqualityComparer))
                    return;
                
                _log.Info("DeploymentAction {deploymentActionKey} changed state {fromStatus}->{toStatus}",
                    change.ActionKey,
                    change.FromStatus,
                    change.ToStatus
                );

                State.Status = change.ToStatus switch
                {
                    DeploymentActionStatus.Created => DeploymentStatus.Created,
                    DeploymentActionStatus.Pending => DeploymentStatus.Queued,
                    DeploymentActionStatus.Starting => DeploymentStatus.Deploying,
                    DeploymentActionStatus.UpdatingManifests => DeploymentStatus.Deploying,
                    DeploymentActionStatus.Synchronizing => DeploymentStatus.Deploying,
                    DeploymentActionStatus.Synchronized => DeploymentStatus.Deploying,
                    DeploymentActionStatus.Failed => DeploymentStatus.Failed,
                    DeploymentActionStatus.Complete => GetDeploymentStatusFromCompletedAction(change.ActionKey),
                };

                await WriteStateAsync();
                await PostOrUpdateSlackMessage();
            });
            
            // subscribe to new deployment events on the deployments namespace (these are sent by the deployment service)
            await SubscribeToPrivateMessaging<NewDeploymentEvent>(
                this.GetPrimaryKey(),
                DeploymentStreamingConstants.DeploymentsNamespace,
                async (e, token) =>
                {
                    using var logScope = _log.BeginShipbotLogScope();
                    
                    // TODO check for reconfiguration
            
                    State.Application = e.ApplicationEnvironment.Application;
                    State.ImageRepository = e.Image.Repository;
                    State.TargetTag = e.TargetTag;
                    State.IsPromotable = e.IsPromotable;
                    State.IsManuallyStarted = e.IsManuallyStarted;

                    // determine if we have other environments we want to promote to.
                    var targetEnvironments = new List<ApplicationEnvironmentKey>()
                    {
                        e.ApplicationEnvironment
                    };
                    
                    targetEnvironments.AddRange(e.PromotableEnvironments
                        .Select(x => new ApplicationEnvironmentKey(State.Application, x))
                    );

                    foreach (var env in targetEnvironments) {
                        var currentTag = e.CurrentTags[env];

                        // build deployment action metadata
                        var deploymentAction = new DeploymentAction()
                        {
                            Image = e.Image,
                            ApplicationEnvironmentKey = env,
                            CurrentTag = currentTag,
                            TargetTag = e.TargetTag
                        };
                        
                        // create deployment key
                        var key = new DeploymentActionKey(Guid.NewGuid());
                        State.DeploymentActions.Add(key);
                        
                        // configure deployment action
                        var deploymentActionGrain = GrainFactory.GetDeploymentActionGrain(key);
                        await deploymentActionGrain.Configure(deploymentAction);
                        await deploymentActionGrain.SetParentDeploymentKey(new DeploymentKey(this.GetPrimaryKey()));
                    }

                    await WriteStateAsync();
                    
                    await PostOrUpdateSlackMessage();
                });
            
            await base.OnActivateAsync();
        }

        private DeploymentStatus GetDeploymentStatusFromCompletedAction(DeploymentActionKey changeActionKey)
        {
            var deploymentActionIndex = State.DeploymentActions.IndexOf(changeActionKey);

            return State.IsPromotable && deploymentActionIndex == 0
                ? DeploymentStatus.Waiting
                : DeploymentStatus.Completed;
        }

        private async Task<SlackMessage> BuildDeploymentUpdateMessage()
        {
            var deployment = await GetDeploymentInformation();
            
            var blocks = new List<IBlock>()
            {
                new SectionBlock()
                {
                    text = new Text()
                    {
                        type = "mrkdwn",
                        text = $"A new image for *{deployment.Application}* was created on the container registry.\n" +
                               $"Image Repository: {deployment.ContainerRepository}\n" +
                               $"Tag: *{deployment.TargetTag}*."
                    }
                },
                new DividerBlock()
            };

            var deploymentActionGrains = new List<DeploymentActionKey>(await GetDeploymentActionIds())
                .Select( (deploymentActionId, index) => (GrainFactory.GetDeploymentActionGrain(deploymentActionId), deploymentActionId, index) );
            
            
            foreach (var deploymentActionGrain in deploymentActionGrains)
            {
                var deploymentAction = await deploymentActionGrain.Item1.GetAction();
                var deploymentActionIndex = deploymentActionGrain.index;
                var deploymentActionId = deploymentActionGrain.deploymentActionId;

                var slackMessageFields = new Text[]
                {
                    new Text()
                    {
                        text = deploymentAction.Status == DeploymentActionStatus.Complete ? $"*Previous Tag*\n{deploymentAction.CurrentTag}" : $"*Current Tag*\n{deploymentAction.CurrentTag}",
                        type = "mrkdwn"
                    },
                    new Text()
                    {
                        text = $"*Status*\n{deploymentAction.Status}",
                        type = "mrkdwn"
                    }
                };
                
                // 
                var deploymentActionBlock = deploymentAction.Status switch
                {
                    DeploymentActionStatus.Created => new SectionBlock()
                    {
                        text = new Text()
                        {
                            type = "mrkdwn",
                            text =
                                $"Scheduled deployment to environment *'{deploymentAction.ApplicationEnvironmentKey.Environment}'*."
                        },
                        fields = slackMessageFields
                    },
                    DeploymentActionStatus.Pending => new SectionBlock()
                    {
                        text = new Text()
                        {
                            type = "mrkdwn",
                            text =
                                $"Starting deployment to environment *'{deploymentAction.ApplicationEnvironmentKey.Environment}'*."
                        },
                        fields = slackMessageFields
                    },
                    DeploymentActionStatus.Starting => new SectionBlock()
                    {
                        text = new Text()
                        {
                            type = "mrkdwn",
                            text = deploymentActionIndex == 0 
                                ? $"Starting deployment to environment *'{deploymentAction.ApplicationEnvironmentKey.Environment}'*."
                                : $"Promoting image to environment *'{deploymentAction.ApplicationEnvironmentKey.Environment}'*."
                                
                        },
                        fields = slackMessageFields
                    },
                    DeploymentActionStatus.Complete => new SectionBlock()
                    {
                        text = new Text()
                        {
                            type = "mrkdwn",
                            text =
                                $"Deployment to environment *'{deploymentAction.ApplicationEnvironmentKey.Environment}'* complete."
                        },
                        fields = slackMessageFields
                    },
                    _ => new SectionBlock()
                    {
                        text = new Text()
                        {
                            type = "mrkdwn",
                            text =
                                $"Deployment to environment *'{deploymentAction.ApplicationEnvironmentKey.Environment}'*."
                        }, 
                        fields = slackMessageFields
                    }
                };

                if (deploymentActionIndex == State.NextDeploymentActionIndex)
                {
                    // TODO: add accessories to next action
                    
                }
                
                blocks.Add(deploymentActionBlock);
            }

            var addActionsBar = (
                (State.NextDeploymentActionIndex == 0 && State.IsManuallyStarted) ||
                (State.NextDeploymentActionIndex >= 0 && State.IsPromotable)
            );
            
            if (addActionsBar)
            {
                var buttons = new List<ButtonElement>();


                buttons.Add(
                    new ButtonElement()
                    {
                        action_id = "deploy",
                        value = State.DeploymentActions[State.NextDeploymentActionIndex],
                        text = new Text()
                        {
                            text = State.NextDeploymentActionIndex == 0 ? "Deploy" : "Promote"
                        }
                    }
                );

                buttons.Add(
                    new ButtonElement()
                    {
                        action_id = "revert",
                        text = new Text()
                        {
                            type = "plain_text",
                            text = "Revert this deployment."
                        },
                        style = "danger",
                        value = "revert",
                    }
                );
                
                blocks.AddRange(
                    new IBlock[]
                    {
                        new DividerBlock(),
                        new ActionsBlock()
                        {
                            elements = buttons.Cast<IElement>().ToArray()
                        }
                    }
                );
            }

            return new SlackMessage(
                $"A new image of *{deployment.ContainerRepository}* was detected with tag *{deployment.TargetTag}*.",
                blocks.ToArray()
            );
        }


        public Task<(string Application, string ContainerRepository, string TargetTag, DeploymentStatus Status)> GetDeploymentInformation() =>
            Task.FromResult(
                (State.Application!.Name, State.ImageRepository!, State.TargetTag!, State.Status)
            );

        public async Task SubmitNextDeploymentAction()
        {
            // TODO: handle multiple items in the deployment plan
            using (_log.BeginShipbotLogScope(State.Application))
            {
                var deploymentActionKey = State.DeploymentActions[State.NextDeploymentActionIndex];

                await PostOrUpdateSlackMessage();
                
                // move pointer to next deployment action
                State.NextDeploymentActionIndex++;

                var deploymentQueue = GrainFactory.GetDeploymentQueueGrain();
                await deploymentQueue.QueueDeploymentAction(deploymentActionKey);
                
                await WriteStateAsync();
            }
        }

        private async Task PostOrUpdateSlackMessage()
        {
            var deploymentUpdateMessage = await BuildDeploymentUpdateMessage();

            if (State.SlackMessageHandle == null)
            {
                State.SlackMessageHandle = await _slackClient.PostMessageAsync(
                    "slack-bots-and-more",
                    deploymentUpdateMessage
                );
            }
            else
            {
                await _slackClient.UpdateMessageAsync(
                    State.SlackMessageHandle,
                    deploymentUpdateMessage
                );
            }

            await WriteStateAsync();
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
    }
}