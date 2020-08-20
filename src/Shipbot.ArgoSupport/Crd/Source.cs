using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Crd
{
    public class Source
    {
        [JsonProperty("helm")]
        public Helm Helm { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("repoURL")]
        public string RepoUrl { get; set; }
        [JsonProperty("targetRevision")]
        public string TargetRevision { get; set; }
    }
}