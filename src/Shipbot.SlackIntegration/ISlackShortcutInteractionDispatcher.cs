using System.Threading.Tasks;
using Slack.NetStandard.Interaction;

namespace Shipbot.SlackIntegration
{
    public interface ISlackShortcutInteractionDispatcher : ISlackPayloadDispatcher<ShortcutPayload>
    {
    }
}