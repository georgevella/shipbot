using System;
using System.Threading.Tasks;
using Slack.NetStandard.EventsApi.CallbackEvents;

namespace Shipbot.SlackIntegration.Events
{
    public abstract class BaseSlackEventHandler<T> : ISlackEventHandler<T> 
        where T : ICallbackEvent
    {
        Task ISlackPayloadProcessor<ICallbackEvent>.Process(ICallbackEvent callbackEvent)
        {
            if (callbackEvent is T typedEvent)
                return Process(typedEvent);
            
            throw new InvalidOperationException("Incorrect type");
        }

        public abstract Task Process(T callbackEvent);
    }
}