using System.Collections.Generic;
using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Crd
{
    public class Helm
    {
        [JsonProperty("valueFiles")]
        public List<string> ValueFiles { get; set; }
    }
}