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
using Shipbot.Controller.Core.Slack;
using Shipbot.Controller.Core.Utilities;
using Shipbot.Controller.Core.Utilities.Eventing;
using SlackAPI;
using SlackClient = SlackAPI.SlackClient;

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
            await SubscribeForEvents<DeploymentActionStatusChangeEvent>((change, token) =>
            {
                // do we know about this deployment action
                if (!State.DeploymentActions.Contains(change.ActionKey, DeploymentActionKey.EqualityComparer))
                    return Task.CompletedTask;
                
                _log.Info("DeploymentAction {deploymentActionKey} changed state {fromStatus}->{toStatus}",
                    change.ActionKey,
                    change.FromStatus,
                    change.ToStatus
                );
                
                return PostOrUpdateSlackMessage();
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
                });
            
            await base.OnActivateAsync();
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
                        text =
                            $"*{deployment.Application}*: A new image of *{deployment.ContainerRepository}* was detected with tag *{deployment.TargetTag}*."
                    }
                },
                new DividerBlock()
            };

            var deploymentActionIds = await GetDeploymentActionIds();
            
            foreach (var deploymentActionId in deploymentActionIds)
            {
                var deploymentActionGrain = GrainFactory.GetDeploymentActionGrain(deploymentActionId);

                var deploymentAction = await deploymentActionGrain.GetAction();

                var slackMessageFields = new Text[]
                {
                    new Text()
                    {
                        text = $"*Current Tag*\n{deploymentAction.CurrentTag}",
                        type = "mrkdwn"
                    },
                    new Text()
                    {
                        text = $"*Status*\n{deploymentAction.Status}",
                        type = "mrkdwn"
                    }
                };
                
                // if (!deploymentAction.IsTriggeredByPromotion)
                {
                    blocks.Add(
                        new SectionBlock()
                        {
                            text = new Text()
                            {
                                type = "mrkdwn",
                                text =
                                    $"Scheduled deployment to environment *'{deploymentAction.ApplicationEnvironmentKey.Environment}'*."
                            },
                            fields = slackMessageFields
                        }
                        );
                }
                // else
                // {
                //     blocks.Add(
                //         new SectionBlock()
                //         {
                //             text = new Text()
                //             {
                //                 type = "mrkdwn",
                //                 text =
                //                     $"Promoting deployment from environment *'{deploymentAction.SourceDeploymentUpdate.Environment.Name}'* to environment *'{deploymentAction.Environment.Name}'*."
                //             },
                //             fields = slackMessageFields
                //         }
                //     );
                // }
                //
                // blocks.Add(
                //         new DividerBlock()
                // );
                //
                // if (deploymentUpdate.status == DeploymentActionStatus.Complete)
                // {
                //     if (deploymentUpdate.Environment.PromotionEnvironments.Count > 0)
                //     {
                //         var slackPromoteActionDetails = new SlackPromoteActionDetails()
                //         {
                //             Application = deploymentUpdate.Application.Name,
                //             ContainerRepository = deploymentUpdate.Image.Repository,
                //             SourceEnvironment = deploymentUpdate.Environment.Name,
                //             TargetTag = deploymentUpdate.TargetTag
                //         };
                //         var buttons = deploymentUpdate.Environment.PromotionEnvironments.Select(x => new ButtonElement()
                //         {
                //             action_id = "promote",
                //             text = new Text()
                //             {
                //                 type = "plain_text",
                //                 text = $"Promote to '{x}'"
                //             },
                //             value = $"{JsonConvert.SerializeObject(slackPromoteActionDetails, Formatting.None)}",
                //         }).ToList();
                //         
                //         buttons.Add(new ButtonElement()
                //         {
                //             action_id = "revert",
                //             text = new Text()
                //             {
                //                 type = "plain_text",
                //                 text = "Revert this deployment."
                //             },
                //             style = "danger",
                //             value = "revert",
                //         });
                //
                //         blocks.AddRange(
                //             new IBlock[]
                //             {
                //                 new ActionsBlock()
                //                 {
                //                     elements = buttons.Cast<IElement>().ToArray()
                //                 }
                //             }
                //         );           
                //     }
                // }
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