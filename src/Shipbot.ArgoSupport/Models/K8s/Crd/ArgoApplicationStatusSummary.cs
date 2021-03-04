using System.Collections.Generic;
using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    public class ArgoApplicationStatusSummary
    {
        protected bool Equals(ArgoApplicationStatusSummary other)
        {
            return ExternalUrls.IsCollectionEqualTo(other.ExternalUrls);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ArgoApplicationStatusSummary) obj);
        }

        public override int GetHashCode()
        {
            return (ExternalUrls != null ? ExternalUrls.GetCollectionHashCode() : 0);
        }

        public ArgoApplicationStatusSummary(List<string> externalUrls)
        {
            ExternalUrls = externalUrls;
        }

        [JsonProperty("externalURLs")] public List<string> ExternalUrls { get; }
    }
}