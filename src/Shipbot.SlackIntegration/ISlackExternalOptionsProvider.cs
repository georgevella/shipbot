using System.Collections.Generic;
using Shipbot.SlackIntegration.Dto.Slack.ExternalOptions;
using Slack.NetStandard.Messages.Elements;

namespace Shipbot.SlackIntegration
{
    public interface ISlackExternalOptionsProvider : ISlackPayloadDispatcher<BlockSuggestionPayload, IEnumerable<IOption>>
    {
    }
}