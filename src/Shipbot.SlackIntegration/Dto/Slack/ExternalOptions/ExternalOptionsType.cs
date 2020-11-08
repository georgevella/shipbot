using System.Runtime.Serialization;

namespace Shipbot.SlackIntegration.Dto.Slack.ExternalOptions
{
    public enum ExternalOptionsType
    {
        [EnumMember(Value = "block_suggestion")]
        BlockSuggestion
    }
}