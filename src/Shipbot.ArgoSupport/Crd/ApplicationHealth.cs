using Newtonsoft.Json;

namespace ArgoAutoDeploy.Core.Argo.Crd
{
    public class ApplicationHealth
    {
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}