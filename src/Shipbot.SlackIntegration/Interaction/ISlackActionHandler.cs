using Slack.NetStandard.Interaction;

namespace Shipbot.SlackIntegration.Interaction
{
    public interface ISlackActionHandler : ISlackPayloadProcessor<SlackAction>
    {
    }
}