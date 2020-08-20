using System.Collections.Generic;
using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Crd
{
    public class ApplicationStatus
    {
        [JsonProperty("health")]
        public ApplicationHealth Health { get; set; }
        
        [JsonProperty("history")]
        public IEnumerable<ApplicationHistoryItem> History { get; set; }
    }
}