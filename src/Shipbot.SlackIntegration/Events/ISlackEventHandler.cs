using System.Threading.Tasks;
using Slack.NetStandard.EventsApi.CallbackEvents;

namespace Shipbot.SlackIntegration.Events
{
    public interface ISlackEventHandler : ISlackPayloadProcessor<ICallbackEvent>
    {
        
    }
    
    public interface ISlackEventHandler<in T> : ISlackEventHandler
        where T: ICallbackEvent
    {
        public Task Process(T callbackEvent);
    }
}