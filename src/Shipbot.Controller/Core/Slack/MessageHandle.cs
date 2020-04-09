using System.Collections.Generic;
using SlackAPI;

namespace Shipbot.Controller.Core.Slack
{
    internal class SingleMessageHandle : IMessageHandle
    {
        public SingleMessageHandle(string timestamp, string channelId)
        {
            Timestamp = timestamp;
            ChannelId = channelId;
        }

        public SingleMessageHandle(PostMessageResponse messageResponse) 
            : this(messageResponse.ts, messageResponse.channel) 
        {
        }

        public SingleMessageHandle(UpdateResponse messageResponse) 
            : this(messageResponse.ts, messageResponse.channel)
        {
        }

        public string Timestamp { get; }
        
        public string ChannelId { get; }
    }

    public interface IMessageHandle
    {
        
    }
}