using Newtonsoft.Json;

namespace ArgoAutoDeploy.Core.Argo.Crd
{
    public class Automated
    {
        [JsonProperty("prune")]
        public bool Prune { get; set; }
    }
}