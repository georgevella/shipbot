using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Models;
using SlackAPI;

namespace Shipbot.SlackIntegration
{
    public class SlackClient : ISlackClient, IDisposable
    {
        private readonly ILogger<SlackClient> _log;
        private readonly IOptions<SlackConfiguration> _slackConfiguration;
        private readonly SlackTaskClient _actualClient;
        private int _timeout;

        public SlackClient(
            ILogger<SlackClient> log,
            IOptions<SlackConfiguration> slackConfiguration
        )
        {
            _log = log;
            _slackConfiguration = slackConfiguration;
            _actualClient = new SlackTaskClient(_slackConfiguration.Value.Token);
        }

        public async Task Connect()
        {
            _timeout = _slackConfiguration.Value.Timeout;
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

//        private Task<IEnumerable<Channel>> GetPublicChannels()
//        {
//            var tsc = new TaskCompletionSource<IEnumerable<Channel>>();
//
//            try
//            {
//                _actualClient.GetChannelList(
//                    response =>
//                    {
//                        if (response.ok)
//                        {
//                            tsc.SetResult(new ReadOnlyCollection<Channel>(response.channels));
//                        }
//                        else
//                        {
//                            tsc.SetException(new InvalidOperationException($"SLACKCLIENT ERROR: {response.error}"));
//                        }
//                    }
//                );
//            }
//            catch (Exception e)
//            {
//                tsc.SetException(e);
//            }
//
//            return tsc.Task;
//        }
//        
//        private Task<IEnumerable<Channel>> GetPrivateChannels()
//        {
//            var tsc = new TaskCompletionSource<IEnumerable<Channel>>();
//
//            try
//            {
//                _actualClient.GetGroupsListAsync()
//                _actualClient.GetGroupsListAsync(
//                    response =>
//                    {
//                        if (response.ok)
//                        {
//                            tsc.SetResult(new ReadOnlyCollection<Channel>(response.groups));
//                        }
//                        else
//                        {
//                            tsc.SetException(new InvalidOperationException($"SLACKCLIENT ERROR: {response.error}"));
//                        }
//                    }
//                );
//            }
//            catch (Exception e)
//            {
//                tsc.SetException(e);
//            }
//
//            return tsc.Task;
//        }

//        private async Task<IEnumerable<Channel>> GetChannels()
//        {
//            var publicChannels = await GetPublicChannels();
//            var privateChannels = await GetPrivateChannels();
//
//            return privateChannels.Concat(publicChannels).ToArray();
//        }

        public async Task<IMessageHandle> PostMessageAsync(string channelId, SlackMessage message)
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

        private SlackMessage BuildDeploymentUpdateMessage(DeploymentUpdate deploymentUpdate, DeploymentUpdateStatus status)
        {
            return new SlackMessage(
                $"A new image of *{deploymentUpdate.Image.Repository}* was detected (tag *{deploymentUpdate.TargetTag}*).",
                new IBlock[]
                {
                    new SectionBlock()
                    {
                        text = new Text()
                        {
                            type = "mrkdwn",
                            text =
                                $"A new image of *{deploymentUpdate.Image.Repository}* was detected (tag *{deploymentUpdate.TargetTag}*)."
                        }
                    },
                    new DividerBlock(),
                    new SectionBlock()
                    {
                        fields = new Text[]
                        {
                            new Text()
                            {
                                text = $"*From*\n{deploymentUpdate.CurrentTag}",
                                type = "mrkdwn"
                            },
                            new Text()
                            {
                                text = $"*To*\n{deploymentUpdate.TargetTag}",
                                type = "mrkdwn"
                            },
                        }
                    },
                    new SectionBlock()
                    {
                        fields = new Text[]
                        {
                            new Text()
                            {
                                text = $"*Application*\n{deploymentUpdate.Application}",
                                type = "mrkdwn"
                            },
                            new Text()
                            {
                                text = $"*Status*\n{status}",
                                type = "mrkdwn"
                            },
                        }
                    }
                }
            );
        }

        public async Task<IMessageHandle> SendDeploymentUpdateNotification(
            string channel, 
            DeploymentUpdate deploymentUpdate,
            DeploymentUpdateStatus status
            )
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (deploymentUpdate == null) throw new ArgumentNullException(nameof(deploymentUpdate));

//            var channelsResponse = await _actualClient.APIRequestWithTokenAsync<ConversationsListResponse>(
//                new Tuple<string, string>("exclude_archived", "true"),
//                new Tuple<string, string>("types", "public_channel,private_channel")
//            );
//            
//            var channelMetadata = channelsResponse.channels.FirstOrDefault(c =>
//                c.name.Equals(channel, StringComparison.OrdinalIgnoreCase)
//            );
//            
//            if (channelMetadata == null)
//            {
//                throw new Exception($"Could not find channel with name {channel}");
//            }

            return await PostMessageAsync(
                channel,
                BuildDeploymentUpdateMessage(deploymentUpdate, status)
            );
        }

        public async Task<IMessageHandle> UpdateDeploymentUpdateNotification(
            IMessageHandle handle, 
            DeploymentUpdate deploymentUpdate,
            DeploymentUpdateStatus status
        )
        {
            if (handle == null) throw new ArgumentNullException(nameof(handle));
            if (deploymentUpdate == null) throw new ArgumentNullException(nameof(deploymentUpdate));

            return await UpdateMessageAsync(
                handle as SingleMessageHandle, 
                BuildDeploymentUpdateMessage(deploymentUpdate, status)
            );
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