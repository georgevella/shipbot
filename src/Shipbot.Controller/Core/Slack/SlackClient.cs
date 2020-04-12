using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Octokit;
using Orleans;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Controller.Core.Deployments;
using Shipbot.Controller.Core.Deployments.GrainState;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.Core.Models;
using SlackAPI;

namespace Shipbot.Controller.Core.Slack
{
    public interface ISlackClient : IDisposable
    {
        Task Connect();
        Task<IMessageHandle> SendMessage(string channel, string message);

        Task<IMessageHandle> PostMessageAsync(string channelId, SlackMessage message);

        Task<IMessageHandle> UpdateMessageAsync(IMessageHandle handle, SlackMessage message);
    }
    
    public class SlackClient : ISlackClient, IDisposable
    {
        private readonly ILogger<SlackClient> _log;
        private readonly IOptions<SlackConfiguration> _slackConfiguration;
        private readonly SlackTaskClient _actualClient;
        private int _timeout;
        private IGrainFactory _grainFactory;

        public SlackClient(
            ILogger<SlackClient> log,
            IOptions<SlackConfiguration> slackConfiguration,
            IClusterClient clusterClient
        )
        {
            _log = log;
            _slackConfiguration = slackConfiguration;
            _grainFactory = clusterClient;
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
        
        public async Task<IMessageHandle> UpdateMessageAsync(IMessageHandle handle, SlackMessage message)
        {
            var actualHandle = (SingleMessageHandle) handle;
            var tsc = new TaskCompletionSource<SingleMessageHandle>();
            
            var ct = new CancellationTokenSource(_timeout);
            ct.Token.Register(() => tsc.TrySetCanceled(), useSynchronizationContext: false);
            
            _log.LogInformation($"Sending message update for [{actualHandle.Timestamp}/${actualHandle.ChannelId}]");
            var response = await UpdateWithBlocksAsync(
                actualHandle.Timestamp,
                actualHandle.ChannelId,
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