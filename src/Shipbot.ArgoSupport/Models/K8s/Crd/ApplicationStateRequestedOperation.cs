using System.Collections.Generic;
using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    public class ApplicationStateRequestedOperation 
    {
        protected bool Equals(ApplicationStateRequestedOperation other)
        {
            return Info.IsCollectionEqualTo(other.Info) && 
                   Equals(InitiatedBy, other.InitiatedBy) &&
                   Equals(Retry, other.Retry) && 
                   Equals(Sync, other.Sync);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationStateRequestedOperation) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Info != null ? Info.GetCollectionHashCode() : 0);
                hashCode = (hashCode * 397) ^ (InitiatedBy != null ? InitiatedBy.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Retry != null ? Retry.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Sync != null ? Sync.GetHashCode() : 0);
                return hashCode;
            }
        }

        public ApplicationStateRequestedOperation(
            List<BaseNameValueModel> info, 
            dynamic initiatedBy, 
            dynamic retry, 
            ArgoApplicationSyncOperation sync
        )
        {
            Info = info;
            InitiatedBy = initiatedBy;
            Retry = retry;
            Sync = sync;
        }

        [JsonProperty("info")] public List<BaseNameValueModel> Info { get; }
        [JsonProperty("initiatedBy")] public dynamic InitiatedBy { get; }
        [JsonProperty("retry")] public dynamic Retry { get; }
        [JsonProperty("sync")] public ArgoApplicationSyncOperation Sync { get; }
    }
}