using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Controller.Core.Models;
using SlackAPI;

namespace Shipbot.Controller.Core.Slack
{
    public class SlackClient : ISlackClient, IDisposable
    {
        private readonly ILogger<SlackClient> _log;
        private readonly IOptions<SlackConfiguration> _slackConfiguration;
        private SlackSocketClient _actualClient;
        private int _timeout;

        public SlackClient(
            ILogger<SlackClient> log,
            IOptions<SlackConfiguration> slackConfiguration
        )
        {
            _log = log;
            _slackConfiguration = slackConfiguration;
        }

        public void Connect()
        {
            _timeout = _slackConfiguration.Value.Timeout;
            
            _actualClient = new SlackSocketClient(_slackConfiguration.Value.Token);
            _actualClient.Connect(
                response =>
                {
                    _log.LogInformation("Connection to slack established.");
                },
                () =>
                {
                    _log.LogInformation("Connection to slack via web-socket established.");
                }
                );

            _actualClient.OnHello += () => { _log.LogInformation("Slack -- hello"); };
            
            _actualClient.OnMessageReceived += message =>
            {
                _log.LogDebug("Slack message received: {slackMessage}", message);
            };

            _actualClient.OnReactionAdded += added =>
            {
                _log.LogDebug("Slack message received: {slackReaction}", added);
            };

            _actualClient.OnPongReceived += pong => { _log.LogDebug("Slack pong received: {pong}", pong); };
        }

        private Task<IEnumerable<Channel>> GetChannels()
        {
            var tsc = new TaskCompletionSource<IEnumerable<Channel>>();

            try
            {
                _actualClient.GetGroupsList(
                    response =>
                    {
                        if (response.ok)
                        {
                            tsc.SetResult(new ReadOnlyCollection<Channel>(response.groups));
                        }
                        else
                        {
                            tsc.SetException(new InvalidOperationException($"SLACKCLIENT ERROR: {response.error}"));
                        }
                    }
                );
            }
            catch (Exception e)
            {
                tsc.SetException(e);
            }

            return tsc.Task;
        }

        private Task<SingleMessageHandle> PostMessageAsync(string channelId, SlackMessage message)
        {
            var tsc = new TaskCompletionSource<SingleMessageHandle>();
            
            var ct = new CancellationTokenSource(_timeout);
            ct.Token.Register(() => tsc.TrySetCanceled(), useSynchronizationContext: false);
            
            _actualClient.PostMessage(
                messageResponse =>
                {
                    _log.LogInformation($"RESPONSE >> Received message deliver for [{messageResponse.ts}/${messageResponse.channel}]");
                    tsc.SetResult(new SingleMessageHandle(messageResponse));
                }, 
                channelId, 
                message.Message,
                blocks: message.Blocks
                );

            return tsc.Task;
        }

        private Task<SingleMessageHandle> UpdateMessageAsync(SingleMessageHandle handle, SlackMessage message)
        {
            var tsc = new TaskCompletionSource<SingleMessageHandle>();
            
            var ct = new CancellationTokenSource(_timeout);
            ct.Token.Register(() => tsc.TrySetCanceled(), useSynchronizationContext: false);
            
            _log.LogInformation($"Sending message update for [{handle.Timestamp}/${handle.ChannelId}]");
            _actualClient.Update(
                response =>
                {
                    _log.LogInformation($"RESPONSE >> Sending message update for [{response.ts}/${response.channel}]");
                    tsc.SetResult(new SingleMessageHandle(response));
                }, 
                handle.Timestamp,
                handle.ChannelId,
                message.Message,
                blocks: message.Blocks 
                );

            return tsc.Task;
        }

        public async Task<IMessageHandle> SendMessage(string channel, string message)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (message == null) throw new ArgumentNullException(nameof(message));
            
            return await PostMessageAsync(channel, new SlackMessage(message));
        }

        private SlackMessage BuildDeploymentUpdateMessage(DeploymentUpdate deploymentUpdate)
        {
            return new SlackMessage(
                $"A new image of *{deploymentUpdate.Image.Repository}* was detected (tag *{deploymentUpdate.Tag}*).",
                new IBlock[]
                {
                    new SectionBlock()
                    {
                        text = new Text()
                        {
                            type = "mrkdwn",
                            text =
                                $"A new image of *{deploymentUpdate.Image.Repository}* was detected (tag *{deploymentUpdate.Tag}*)."
                        }
                    },
                    new DividerBlock(),
                    new SectionBlock()
                    {
                        fields = new Text[]
                        {
                            new Text()
                            {
                                text = $"*Tag*\n{deploymentUpdate.Tag}",
                                type = "mrkdwn"
                            },
                            new Text()
                            {
                                text = $"*Status*\n{deploymentUpdate.Status}",
                                type = "mrkdwn"
                            },
                        }
                    }
                }
            );
        }

        public async Task<IMessageHandle> SendDeploymentUpdateNotification(
            string channel, 
            DeploymentUpdate deploymentUpdate
            )
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (deploymentUpdate == null) throw new ArgumentNullException(nameof(deploymentUpdate));

            var channels = await GetChannels();
            
            var channelMetadata = channels.FirstOrDefault(c =>
                c.name.Equals(channel, StringComparison.OrdinalIgnoreCase)
            );
            
            if (channelMetadata == null)
            {
                throw new Exception($"Could not find channel with name {channel}");
            }

            return await PostMessageAsync(
                channelMetadata.id,
                BuildDeploymentUpdateMessage(deploymentUpdate)
            );
        }

        public async Task<IMessageHandle> UpdateDeploymentUpdateNotification(
            IMessageHandle handle, 
            DeploymentUpdate deploymentUpdate
        )
        {
            if (handle == null) throw new ArgumentNullException(nameof(handle));
            if (deploymentUpdate == null) throw new ArgumentNullException(nameof(deploymentUpdate));

            return await UpdateMessageAsync(
                handle as SingleMessageHandle, 
                BuildDeploymentUpdateMessage(deploymentUpdate)
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
            if (_actualClient.IsConnected)
            {
                _actualClient.CloseSocket();
            }
        }
    }
}