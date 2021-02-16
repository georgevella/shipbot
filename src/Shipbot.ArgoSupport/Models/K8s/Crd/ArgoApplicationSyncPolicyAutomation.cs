using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    public class ArgoApplicationSyncPolicyAutomation
    {
        public ArgoApplicationSyncPolicyAutomation(bool prune)
        {
            Prune = prune;
        }

        [JsonProperty("prune")]
        public bool Prune { get; set; }
    }
}