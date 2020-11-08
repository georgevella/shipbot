using Newtonsoft.Json;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.Objects;

namespace Shipbot.SlackIntegration.Dto.Slack.ExternalOptions
{
    public class BlockSuggestionPayload : ExternalOptionsPayload
    {
        [JsonProperty("action_id")]
        public string ActionId { get; set; }
        
        [JsonProperty("block_id")]
        public string BlockId { get; set; }
        
        [JsonProperty("view", NullValueHandling = NullValueHandling.Ignore)]
        public View View { get; set; }
        
        [JsonProperty("container", NullValueHandling = NullValueHandling.Ignore)]
        public Container Container { get; set; }
    }
}