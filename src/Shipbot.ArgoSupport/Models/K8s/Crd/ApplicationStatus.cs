using System.Collections.Generic;
using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    public class ApplicationStatus
    {
        public ApplicationStatus(
            ResourceHealthStatus health, 
            IEnumerable<ApplicationHistoryItem> history, 
            string observedAt, 
            ApplicationOperationState operationState, 
            string reconciledAt, 
            List<ArgoApplicationRelatedResourceStatus> resources, 
            string sourceType, 
            ArgoApplicationStatusSummary summary, 
            ArgoApplicationSyncStatus syncStatus)
        {
            Health = health;
            //History = history;
            ObservedAt = observedAt;
            OperationState = operationState;
            ReconciledAt = reconciledAt;
            Resources = resources;
            SourceType = sourceType;
            Summary = summary;
            SyncStatus = syncStatus;
        }

        protected bool Equals(ApplicationStatus other)
        {
            return Equals(Health, other.Health) &&
                   //Equals(History, other.History) &&
                   ObservedAt == other.ObservedAt && 
                   Equals(OperationState, other.OperationState) && 
                   ReconciledAt == other.ReconciledAt && 
                   Resources.IsCollectionEqualTo(other.Resources) &&
                   SourceType == other.SourceType && 
                   Equals(Summary, other.Summary) && 
                   Equals(SyncStatus, other.SyncStatus);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationStatus) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Health != null ? Health.GetHashCode() : 0);
                //hashCode = (hashCode * 397) ^ (History != null ? History.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ObservedAt != null ? ObservedAt.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OperationState != null ? OperationState.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ReconciledAt != null ? ReconciledAt.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Resources != null ? Resources.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SourceType != null ? SourceType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Summary != null ? Summary.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SyncStatus != null ? SyncStatus.GetHashCode() : 0);
                return hashCode;
            }
        }

        [JsonProperty("health")] public ResourceHealthStatus Health { get; }
        //[JsonProperty("history")] public IEnumerable<ApplicationHistoryItem> History { get; }
        [JsonProperty("observedAt")] public string ObservedAt { get; }
        [JsonProperty("operationState")] public ApplicationOperationState OperationState { get; }
        [JsonProperty("reconciledAt")] public string ReconciledAt { get; }
        [JsonProperty("resources")] public List<ArgoApplicationRelatedResourceStatus> Resources { get; }
        [JsonProperty("sourceType")] public string SourceType { get; }
        [JsonProperty("summary")] public ArgoApplicationStatusSummary Summary { get; }
        [JsonProperty("sync")] public ArgoApplicationSyncStatus SyncStatus { get; }
    }
}