using System.Collections.Generic;
using Shipbot.SlackIntegration.Dto.Slack.ExternalOptions;
using Slack.NetStandard.Messages.Elements;

namespace Shipbot.SlackIntegration.ExternalOptions
{
    public interface ISlackExternalOptionsSource : ISlackPayloadProcessor<BlockSuggestionPayload, IEnumerable<IOption>>
    {
    }
}