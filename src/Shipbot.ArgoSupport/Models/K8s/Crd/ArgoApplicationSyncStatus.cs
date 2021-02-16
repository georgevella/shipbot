using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    public class ArgoApplicationSyncStatus
    {
        protected bool Equals(ArgoApplicationSyncStatus other)
        {
            return Revision == other.Revision 
                   && Status == other.Status;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ArgoApplicationSyncStatus) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Revision != null ? Revision.GetHashCode() : 0) * 397) ^ (int) Status;
            }
        }

        public ArgoApplicationSyncStatus(string revision, SyncStatusCode status)
        {
            Revision = revision;
            Status = status;
        }

        [JsonProperty("revision")] public string Revision { get; }
        [JsonProperty("status")] public SyncStatusCode Status { get; }
    }
}