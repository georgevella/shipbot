using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Crd
{
    public class Automated
    {
        [JsonProperty("prune")]
        public bool Prune { get; set; }
    }
}