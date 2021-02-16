using System.Collections.Generic;
using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    public class ArgoApplicationSyncOperation : ArgoApplicationSyncResult
    {
        protected bool Equals(ArgoApplicationSyncOperation other)
        {
            return base.Equals(other) && 
                   DryRun == other.DryRun &&
                   Manifests.IsCollectionEqualTo(other.Manifests) && 
                   Prune == other.Prune;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ArgoApplicationSyncOperation) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ DryRun.GetHashCode();
                hashCode = (hashCode * 397) ^ (Manifests != null ? Manifests.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Prune.GetHashCode();
                return hashCode;
            }
        }

        [JsonConstructor]
        public ArgoApplicationSyncOperation(
            List<ArgoApplicationRelatedResource> resources, 
            string revision, 
            ApplicationSource source
        ) : base(resources, revision, source)
        {
        }

        [JsonProperty("dryRun")] public bool DryRun { get; }
        [JsonProperty("manifests")] public List<string> Manifests { get;  }
        [JsonProperty("prune")] public bool Prune { get; }
    }
}