using Newtonsoft.Json;

namespace ArgoAutoDeploy.Core.Argo.Crd
{
    public class Destination
    {
        [JsonProperty("namespace")]
        public string Namespace { get; set; }
        [JsonProperty("server")]
        public string Server { get; set; }
    }
}