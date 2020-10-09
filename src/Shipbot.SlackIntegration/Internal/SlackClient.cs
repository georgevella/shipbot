using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Data;
using Slack.NetStandard;
using Slack.NetStandard.Objects;
using Slack.NetStandard.WebApi.Chat;
using SlackAPI;
using Channel = SlackAPI.Channel;

namespace Shipbot.SlackIntegration.Internal
{
    public class SlackClient : ISlackClient
    {
        private readonly ILogger<SlackClient> _log;
        private readonly IEntityRepository<Dao.SlackMessage> _slackMessageRepository;
        private readonly SlackTaskClient _actualClient;
        private readonly ISlackApiClient _netStandardSlackClient;

        public SlackClient(
            ILogger<SlackClient> log,
            IEntityRepository<Dao.SlackMessage> slackMessageRepository,
            SlackClientWrapper actualClient,
            ISlackApiClient netStandardSlackClient
        )
        {
            _log = log;
            _slackMessageRepository = slackMessageRepository;
            _actualClient = actualClient;
            _netStandardSlackClient = netStandardSlackClient;
        }
        
        private Task<IEnumerable<Channel>> GetPublicChannels()
        {
            return Task.FromResult(_actualClient.Channels.AsEnumerable());
        }
        
        private Task<IEnumerable<Channel>> GetPrivateChannels()
        {
            return Task.FromResult(Enumerable.Empty<Channel>());
        }

        public async Task<IEnumerable<Channel>> GetChannels()
        {
            var publicChannels = await GetPublicChannels();
            var privateChannels = await GetPrivateChannels();

            return privateChannels.Concat(publicChannels).ToArray();
        }

        public async Task<IEnumerable<(string name, string description)>> GetAllUserGroupNames()
        {
            var userGroups = await _netStandardSlackClient.Usergroups.List();

            if (userGroups.OK)
            {
                return userGroups.Usergroups.Select(u => (u.Name, u.Description)).ToList();
            }
            
            throw new Exception("Failed to get user groups");
        }

        public async Task<IMessageHandle> PostMessageAsync(string channelId, IMessage message)
        {
            var actualMessage = (SlackMessage) message;

            var messageResponse = await _actualClient.PostMessageAsync(
                channelId,
                actualMessage.Message,
                blocks: actualMessage.Blocks
            );


            if (!messageResponse.ok || !string.IsNullOrEmpty(messageResponse.error))
            {
                _log.LogError("Failed to send message via Slack [{error}]", messageResponse.error);
                throw new InvalidOperationException($"Failed to send message via Slack [{messageResponse.error}]");
            }
            
            _log.LogTrace(
                $"Sent message via slack [{messageResponse.ts}/${messageResponse.channel}]"
                );

            var dao = await _slackMessageRepository.Add(new Dao.SlackMessage()
            {
                Id = Guid.NewGuid(),
                ChannelId = messageResponse.channel,
                Timestamp = messageResponse.ts,
                CreationDateTime = DateTime.Now,
                UpdatedDateTime = DateTime.Now
            });

            await _slackMessageRepository.Save();

            return new MessageHandle(dao.Id);
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
            var dao = await _slackMessageRepository.Find(handle.Id);

            _log.LogInformation($"Sending message update for [{dao.Timestamp}/${dao.ChannelId}]");
            var response = await UpdateWithBlocksAsync(
                dao.Timestamp,
                dao.ChannelId,
                actualMessage.Message,
                blocks: actualMessage.Blocks , as_user: true
                );
            
            if (!response.ok || !string.IsNullOrEmpty(response.error))
            {
                _log.LogError("Failed to send message update via Slack [{error}]", response.error);
                throw new InvalidOperationException($"Failed to send message via Slack [{response.error}]");
            }

            _log.LogTrace(
                $"Updated message via slack [{response.ts}/${response.channel}]"
            );
            dao.ChannelId = response.channel;
            dao.Timestamp = response.ts;
            dao.UpdatedDateTime = DateTimeOffset.Now;

            await _slackMessageRepository.Save();
            
            return handle;
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