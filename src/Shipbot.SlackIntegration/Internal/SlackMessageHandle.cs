using System;
using SlackAPI;

namespace Shipbot.SlackIntegration.Internal
{
    internal class SlackMessageHandle : IMessageHandle
    {
        protected bool Equals(SlackMessageHandle other)
        {
            return Timestamp == other.Timestamp && ChannelId == other.ChannelId;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SlackMessageHandle) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Timestamp, ChannelId);
        }

        public SlackMessageHandle(string timestamp, string channelId)
        {
            Timestamp = timestamp;
            ChannelId = channelId;
        }
        
        public SlackMessageHandle(SlackMessageHandle postedMessageHandle) 
            : this(postedMessageHandle.Timestamp, postedMessageHandle.ChannelId) 
        {
        }

        public SlackMessageHandle(PostMessageResponse messageResponse) 
            : this(messageResponse.ts, messageResponse.channel) 
        {
        }

        public SlackMessageHandle(UpdateResponse messageResponse) 
            : this(messageResponse.ts, messageResponse.channel)
        {
        }

        public string Timestamp { get; }
        
        public string ChannelId { get; }
    }
}