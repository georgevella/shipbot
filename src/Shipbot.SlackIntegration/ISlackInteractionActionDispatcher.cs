using Shipbot.SlackIntegration.Interaction;
using Slack.NetStandard.Interaction;

namespace Shipbot.SlackIntegration
{
    public interface ISlackInteractionActionDispatcher : ISlackPayloadDispatcher<SlackAction>
    {
    }
}