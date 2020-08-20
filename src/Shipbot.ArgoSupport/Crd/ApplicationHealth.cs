using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Crd
{
    public class ApplicationHealth
    {
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}