using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shipbot.Controller.Core.Configuration;
using SlackAPI;

namespace Shipbot.SlackIntegration.Internal
{
    public class SlackClient : ISlackClient, IDisposable
    {
        private readonly ILogger<SlackClient> _log;
        private readonly IOptions<SlackConfiguration> _slackConfiguration;
        private readonly SlackTaskClient _actualClient;
        private readonly int _timeout;

        public SlackClient(
            ILogger<SlackClient> log,
            IOptions<SlackConfiguration> slackConfiguration,
            SlackClientWrapper actualClient
        )
        {
            _log = log;
            _slackConfiguration = slackConfiguration;
            _actualClient = actualClient;
            _timeout = slackConfiguration.Value.Timeout;
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

        public async Task<IMessageHandle> PostMessageAsync(string channelId, IMessage message)
        {
            var actualMessage = (SlackMessage) message;
            
            var messageResponse = await _actualClient.PostMessageAsync(
                channelId,
                actualMessage.Message,
                blocks: actualMessage.Blocks
            );
                
            _log.LogInformation(
                $"RESPONSE >> Received message deliver for [{messageResponse.ts}/${messageResponse.channel}]");

            return new SlackMessageHandle(messageResponse);
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
        
        public async Task<IMessageHandle> UpdateMessageAsync(IMessageHandle handle, IMessage message)
        {
            var actualMessage = (SlackMessage) message;
            var actualHandle = (SlackMessageHandle) handle;
            
            var tsc = new TaskCompletionSource<SlackMessageHandle>();
            
            var ct = new CancellationTokenSource(_timeout);
            ct.Token.Register(() => tsc.TrySetCanceled(), useSynchronizationContext: false);
            
            _log.LogInformation($"Sending message update for [{actualHandle.Timestamp}/${actualHandle.ChannelId}]");
            var response = await UpdateWithBlocksAsync(
                actualHandle.Timestamp,
                actualHandle.ChannelId,
                actualMessage.Message,
                blocks: actualMessage.Blocks 
                );

            _log.LogInformation($"RESPONSE >> Sending message update for [{response.ts}/${response.channel}]");
            return new SlackMessageHandle(response);
        }

        public async Task<IMessageHandle> SendMessage(string channel, string message)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (message == null) throw new ArgumentNullException(nameof(message));
            
            return await PostMessageAsync(channel, new SlackMessage(message));
        }

        // public async Task<IMessageHandle> SendDeploymentUpdateNotification(
        //     string channel, 
        //     DeploymentUpdate deploymentUpdate,
        //     DeploymentUpdateStatus status
        //     )
        // {
        //     if (channel == null) throw new ArgumentNullException(nameof(channel));
        //     if (deploymentUpdate == null) throw new ArgumentNullException(nameof(deploymentUpdate));
        //
        //     return await PostMessageAsync(
        //         channel,
        //         BuildDeploymentUpdateMessage(deploymentUpdate, status)
        //     );
        // }
        //
        // public async Task<IMessageHandle> UpdateDeploymentUpdateNotification(
        //     IMessageHandle handle, 
        //     DeploymentUpdate deploymentUpdate,
        //     DeploymentUpdateStatus status
        // )
        // {
        //     if (handle == null) throw new ArgumentNullException(nameof(handle));
        //     if (deploymentUpdate == null) throw new ArgumentNullException(nameof(deploymentUpdate));
        //
        //     return await UpdateMessageAsync(
        //         handle as SingleMessageHandle, 
        //         BuildDeploymentUpdateMessage(deploymentUpdate, status)
        //     );
        // }

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