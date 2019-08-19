using Newtonsoft.Json;

namespace OperatorSdk.ApiResources
{
    public class CustomResourceWithSpecAndStatus<TSpec, TStatus> : ResourceWithMetadata
    {
        [JsonProperty("spec")]
        public TSpec Spec { get; set; }
        
        [JsonProperty("status")]
        public TStatus Status { get; set; }
    }
}