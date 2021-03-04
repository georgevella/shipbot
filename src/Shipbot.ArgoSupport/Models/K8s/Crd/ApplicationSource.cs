using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    public class ApplicationSource
    {
        protected bool Equals(ApplicationSource other)
        {
            return Chart == other.Chart && 
                   Equals(Directory, other.Directory) && 
                   Equals(Helm, other.Helm) && 
                   // Equals(Ksonnet, other.Ksonnet) && 
                   // Equals(Kustomize, other.Kustomize) && 
                   // Equals(Plugin, other.Plugin) && 
                   Path == other.Path && 
                   RepoUrl == other.RepoUrl && 
                   TargetRevision == other.TargetRevision;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationSource) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Chart != null ? Chart.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Directory != null ? Directory.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Helm != null ? Helm.GetHashCode() : 0);
                // hashCode = (hashCode * 397) ^ (Ksonnet != null ? Ksonnet.GetHashCode() : 0);
                // hashCode = (hashCode * 397) ^ (Kustomize != null ? Kustomize.GetHashCode() : 0);
                // hashCode = (hashCode * 397) ^ (Plugin != null ? Plugin.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Path != null ? Path.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RepoUrl != null ? RepoUrl.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TargetRevision != null ? TargetRevision.GetHashCode() : 0);
                return hashCode;
            }
        }

        [JsonConstructor]
        public ApplicationSource(
            string chart,
            ApplicationSourceDirectory directory, 
            ApplicationSourceHelm helm, 
            dynamic ksonnet,
            dynamic kustomize,
            dynamic plugin, 
            string path, 
            string repoUrl, 
            string targetRevision
        )
        {
            Chart = chart;
            Directory = directory;
            Helm = helm;
            // Ksonnet = ksonnet;
            // Kustomize = kustomize;
            // Plugin = plugin;
            Path = path;
            RepoUrl = repoUrl;
            TargetRevision = targetRevision;
        }

        [JsonProperty("chart")] public string Chart { get; }
        [JsonProperty("directory")] public ApplicationSourceDirectory Directory { get; }
        [JsonProperty("helm")] public ApplicationSourceHelm Helm { get; }
        
        // [JsonProperty("ksonnet")] public dynamic Ksonnet { get; }
        // [JsonProperty("kustomize")] public dynamic Kustomize { get; }
        // [JsonProperty("plugin")] public dynamic Plugin { get; }
        
        [JsonProperty("path")] public string Path { get; }
        [JsonProperty("repoURL")] public string RepoUrl { get; }
        [JsonProperty("targetRevision")] public string TargetRevision { get; }
    }
}