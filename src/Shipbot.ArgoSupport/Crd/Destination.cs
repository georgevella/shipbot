using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Crd
{
    public class Destination
    {
        [JsonProperty("namespace")]
        public string Namespace { get; set; }
        [JsonProperty("server")]
        public string Server { get; set; }
    }
}