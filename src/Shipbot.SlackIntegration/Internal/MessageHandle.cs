using System;
using SlackAPI;

namespace Shipbot.SlackIntegration.Internal
{
    public class MessageHandle : IMessageHandle
    {
        public MessageHandle(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
}