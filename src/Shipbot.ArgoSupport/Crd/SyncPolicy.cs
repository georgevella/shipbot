using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Crd
{
    public class SyncPolicy
    {
        [JsonProperty("automated")]
        public Automated Automated { get; set; }
    }
}