using System.Collections.Generic;
using Newtonsoft.Json;

namespace ArgoAutoDeploy.Core.Argo.Crd
{
    public class Helm
    {
        [JsonProperty("valueFiles")]
        public List<string> ValueFiles { get; set; }
    }
}