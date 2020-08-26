using System;

namespace Shipbot.SlackIntegration
{
    public interface IMessageHandle
    {
        public Guid Id { get; }
    }

    public interface IMessage
    {
        
    }
}