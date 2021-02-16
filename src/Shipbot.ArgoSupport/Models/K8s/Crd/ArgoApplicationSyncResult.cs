using System.Collections.Generic;
using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    public class ArgoApplicationSyncResult
    {
        protected bool Equals(ArgoApplicationSyncResult other)
        {
            return Resources.IsCollectionEqualTo(other.Resources) && 
                   Revision == other.Revision && 
                   Equals(Source, other.Source);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ArgoApplicationSyncResult) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Resources != null ? Resources.GetCollectionHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Revision != null ? Revision.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Source != null ? Source.GetHashCode() : 0);
                return hashCode;
            }
        }

        public ArgoApplicationSyncResult(
            List<ArgoApplicationRelatedResource> resources, 
            string revision, 
            ApplicationSource source
        )
        {
            Resources = resources;
            Revision = revision;
            Source = source;
        }

        [JsonProperty("resources")] public List<ArgoApplicationRelatedResource> Resources { get;  }
        [JsonProperty("revision")] public string Revision { get; }
        [JsonProperty("source")] public ApplicationSource Source { get;  }
    }
}