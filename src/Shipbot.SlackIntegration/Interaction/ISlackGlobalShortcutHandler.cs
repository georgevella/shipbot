using System.Threading.Tasks;
using Slack.NetStandard.Interaction;

namespace Shipbot.SlackIntegration.Interaction
{
    public interface ISlackGlobalShortcutHandler : ISlackPayloadProcessor<ShortcutPayload>
    {
        
    }
}