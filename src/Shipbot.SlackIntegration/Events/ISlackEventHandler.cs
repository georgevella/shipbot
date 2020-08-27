using System;
using System.Threading.Tasks;
using Slack.NetStandard.EventsApi.CallbackEvents;

namespace Shipbot.SlackIntegration.Events
{
    public interface ISlackEventHandler
    {
        Task Invoke(ICallbackEvent callbackEvent);
    }
    
    public interface ISlackEventHandler<T> : ISlackEventHandler
        where T: ICallbackEvent
    {
        
    }

    public abstract class BaseSlackEventHandler<T> : ISlackEventHandler<T> 
        where T : ICallbackEvent
    {
        Task ISlackEventHandler.Invoke(ICallbackEvent callbackEvent)
        {
            if (callbackEvent is T typedEvent)
                return Invoke(typedEvent);
            
            throw new InvalidOperationException("Incorrect type");
        }

        protected abstract Task Invoke(T callbackEvent);
    }
}