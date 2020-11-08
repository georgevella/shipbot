using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shipbot.SlackIntegration.Dto.JsonConverters;
using Slack.NetStandard;
using Slack.NetStandard.ApiCommon;
using Slack.NetStandard.Objects;

namespace Shipbot.SlackIntegration.Dto.Slack.ExternalOptions
{
    [JsonConverter(typeof(ExternalOptionsPayloadConverter))]
    public class ExternalOptionsPayload
    {
        [JsonProperty("type"), JsonConverter(typeof(StringEnumConverter))]
        public ExternalOptionsType Type { get; set; }

        [JsonProperty("user")]
        public UserSummary User { get; set; }

        [JsonProperty("team")]
        public TeamSummary Team { get; set; }

        [JsonProperty("api_app_id", NullValueHandling = NullValueHandling.Ignore)]
        public string ApiAppId { get; set; }

        [JsonProperty("token",NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; set; }
        
        [JsonExtensionData]
        public Dictionary<string,object> OtherFields { get; set; }
    }
}