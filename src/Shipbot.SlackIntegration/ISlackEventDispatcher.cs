using System.Threading.Tasks;
using Slack.NetStandard.EventsApi.CallbackEvents;

namespace Shipbot.SlackIntegration
{
    public interface ISlackEventDispatcher : ISlackPayloadDispatcher<ICallbackEvent>
    {
    }
}