using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Controller.Core.Deployments;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.Core.Models;
using SlackAPI;

namespace Shipbot.Controller.Core.Slack
{
    public class SlackClient : ISlackClient, IDisposable
    {
        private readonly ILogger<SlackClient> _log;
        private readonly IOptions<SlackConfiguration> _slackConfiguration;
        private SlackTaskClient _actualClient;
        private int _timeout;

        public SlackClient(
            ILogger<SlackClient> log,
            IOptions<SlackConfiguration> slackConfiguration 
        )
        {
            _log = log;
            _slackConfiguration = slackConfiguration;
        }

        public async Task Connect()
        {
            _timeout = _slackConfiguration.Value.Timeout;
            
            _actualClient = new SlackTaskClient(_slackConfiguration.Value.Token);
            var loginResponse = await _actualClient.ConnectAsync();

            if (loginResponse.ok)
            {
                _log.LogInformation("Connection to slack established.");
            }
            else
            {
                throw new InvalidOperationException(loginResponse.error);
            }
        }

        private async Task<SingleMessageHandle> PostMessageAsync(string channelId, SlackMessage message)
        {
            var messageResponse = await _actualClient.PostMessageAsync(
                channelId,
                message.Message,
                blocks: message.Blocks
            );
                
            _log.LogInformation(
                $"RESPONSE >> Received message deliver for [{messageResponse.ts}/${messageResponse.channel}]");

            return new SingleMessageHandle(messageResponse);
        }

        private Task<UpdateResponse> UpdateWithBlocksAsync(
            string ts,
            string channelId,
            string text,
            string botName = null,
            string parse = null,
            bool linkNames = false,
            IBlock[] blocks = null,
            Attachment[] attachments = null,
            // ReSharper disable once InconsistentNaming
            bool as_user = false)
        {
            var tupleList = new List<Tuple<string, string>>
            {
                new Tuple<string, string>(nameof(ts), ts),
                new Tuple<string, string>("channel", channelId),
                new Tuple<string, string>(nameof(text), text)
            };
            
            if (!string.IsNullOrEmpty(botName))
                tupleList.Add(new Tuple<string, string>("username", botName));
            
            if (!string.IsNullOrEmpty(parse))
                tupleList.Add(new Tuple<string, string>(nameof (parse), parse));
            
            if (linkNames)
                tupleList.Add(new Tuple<string, string>("link_names", "1"));
            
            if (blocks != null && blocks.Length != 0)
                tupleList.Add(new Tuple<string, string>(nameof (blocks), JsonConvert.SerializeObject(blocks, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                })));
            
            if (attachments != null && attachments.Length != 0)
                tupleList.Add(new Tuple<string, string>(nameof (attachments), JsonConvert.SerializeObject(attachments)));
            
            tupleList.Add(new Tuple<string, string>(nameof (as_user), as_user.ToString()));
            return _actualClient.APIRequestWithTokenAsync<UpdateResponse>(tupleList.ToArray());
        }
        
        private async Task<SingleMessageHandle> UpdateMessageAsync(SingleMessageHandle handle, SlackMessage message)
        {
            var tsc = new TaskCompletionSource<SingleMessageHandle>();
            
            var ct = new CancellationTokenSource(_timeout);
            ct.Token.Register(() => tsc.TrySetCanceled(), useSynchronizationContext: false);
            
            _log.LogInformation($"Sending message update for [{handle.Timestamp}/${handle.ChannelId}]");
            var response = await UpdateWithBlocksAsync(
                handle.Timestamp,
                handle.ChannelId,
                message.Message,
                blocks: message.Blocks 
                );

            _log.LogInformation($"RESPONSE >> Sending message update for [{response.ts}/${response.channel}]");
            return new SingleMessageHandle(response);
        }

        public async Task<IMessageHandle> SendMessage(string channel, string message)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (message == null) throw new ArgumentNullException(nameof(message));
            
            return await PostMessageAsync(channel, new SlackMessage(message));
        }

        private SlackMessage BuildDeploymentUpdateMessage(Deployment deployment)
        {
            var blocks = new List<IBlock>()
            {
                new SectionBlock()
                {
                    text = new Text()
                    {
                        type = "mrkdwn",
                        text =
                            $"*{deployment.Application.Name}*: A new image of *{deployment.ContainerRepository}* was detected with tag *{deployment.TargetTag}*."
                    }
                },
                new DividerBlock()
            };
            
            
            
            foreach (var pair in deployment.GetDeploymentUpdates())
            {
                var deploymentUpdate = pair.DeploymentUpdate;
                var deploymentUpdateStatus = pair.DeploymentUpdateStatus;

                var slackMessageFields = new Text[]
                {
                    new Text()
                    {
                        text = $"*Current Tag*\n{deploymentUpdate.CurrentTag}",
                        type = "mrkdwn"
                    },
                    new Text()
                    {
                        text = $"*Status*\n{deploymentUpdateStatus}",
                        type = "mrkdwn"
                    }
                };
                
                if (!deploymentUpdate.IsTriggeredByPromotion)
                {
                    blocks.Add(
                        new SectionBlock()
                        {
                            text = new Text()
                            {
                                type = "mrkdwn",
                                text =
                                    $"Scheduled deployment to environment *'{deploymentUpdate.Environment.Name}'*."
                            },
                            fields = slackMessageFields
                        }
                        );
                }
                else
                {
                    blocks.Add(
                        new SectionBlock()
                        {
                            text = new Text()
                            {
                                type = "mrkdwn",
                                text =
                                    $"Promoting deployment from environment *'{deploymentUpdate.SourceDeploymentUpdate.Environment.Name}'* to environment *'{deploymentUpdate.Environment.Name}'*."
                            },
                            fields = slackMessageFields
                        }
                    );
                }
                
                blocks.Add(
                        new DividerBlock()
                );

                if (deploymentUpdateStatus == DeploymentUpdateStatus.Complete)
                {
                    if (deploymentUpdate.Environment.PromotionEnvironments.Count > 0)
                    {
                        blocks.AddRange(
                            new IBlock[]
                            {
                                new ActionsBlock()
                                {
                                    elements = new IElement[]
                                    {
                                        new ButtonElement()
                                        {
                                            action_id = "promote",
                                            text = new Text()
                                            {
                                                type = "plain_text",
                                                text = "Promote to Staging"
                                            },
                                            value = "staging",
                                        },
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
                                    }
                                }
                            }
                        );           
                    }
                }
            }

            return new SlackMessage(
                $"A new image of *{deployment.ContainerRepository}* was detected with tag *{deployment.TargetTag}*.",
                blocks.ToArray()
            );
        }

        public async Task<IMessageHandle> SendDeploymentUpdateNotification(
            string channel, 
            Deployment deployment
            )
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (deployment == null) throw new ArgumentNullException(nameof(deployment));

            return await PostMessageAsync(
                channel,
                BuildDeploymentUpdateMessage(deployment)
            );
        }

        public async Task<IMessageHandle> UpdateDeploymentUpdateNotification(
            IMessageHandle handle, 
            Deployment deployment
        )
        {
            if (handle == null) throw new ArgumentNullException(nameof(handle));
            if (deployment == null) throw new ArgumentNullException(nameof(deployment));

            return await UpdateMessageAsync(
                handle as SingleMessageHandle, 
                BuildDeploymentUpdateMessage(deployment)
            );
        }

        class SlackMessage
        {
            public string Message { get; }
            
            public IBlock[] Blocks { get; }

            public SlackMessage(string message, IBlock[] blocks = null)
            {
                Message = message;
                Blocks = blocks;
            }
        }

        public void Dispose()
        {

        }
    }
    
    [RequestPath("conversations.list", true)]
    public class ConversationsListResponse : Response
    {
        // ReSharper disable once InconsistentNaming
        public Channel[] channels;
    }
}