using Newtonsoft.Json;

namespace ArgoAutoDeploy.Core.Argo.Crd
{
    public class ApplicationSpec
    {
        [JsonProperty("destination")]
        public Destination Destination { get; set; }
        [JsonProperty("project")]
        public string Project { get; set; }
        [JsonProperty("source")]
        public Source Source { get; set; }
        [JsonProperty("syncPolicy")]
        public SyncPolicy SyncPolicy { get; set; }
    }
}