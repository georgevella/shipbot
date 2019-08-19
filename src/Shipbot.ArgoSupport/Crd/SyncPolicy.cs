using Newtonsoft.Json;

namespace ArgoAutoDeploy.Core.Argo.Crd
{
    public class SyncPolicy
    {
        [JsonProperty("automated")]
        public Automated Automated { get; set; }
    }
}