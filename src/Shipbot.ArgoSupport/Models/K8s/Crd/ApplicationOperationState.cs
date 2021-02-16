using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    public class ApplicationOperationState
    {
        protected bool Equals(ApplicationOperationState other)
        {
            return FinishedAt == other.FinishedAt &&
                   Message == other.Message && 
                   Equals(RequestedOperation, other.RequestedOperation) && 
                   Phase == other.Phase && 
                   RetryCount == other.RetryCount && 
                   StartedAt == other.StartedAt && 
                   Equals(SyncResult, other.SyncResult);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationOperationState) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (FinishedAt != null ? FinishedAt.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Message != null ? Message.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RequestedOperation != null ? RequestedOperation.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Phase != null ? Phase.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RetryCount != null ? RetryCount.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (StartedAt != null ? StartedAt.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SyncResult != null ? SyncResult.GetHashCode() : 0);
                return hashCode;
            }
        }

        public ApplicationOperationState(
            string finishedAt, 
            string message,
            ApplicationStateRequestedOperation requestedOperation, 
            string phase, 
            string retryCount, 
            string startedAt, 
            ArgoApplicationSyncResult syncResult
        )
        {
            FinishedAt = finishedAt;
            Message = message;
            RequestedOperation = requestedOperation;
            Phase = phase;
            RetryCount = retryCount;
            StartedAt = startedAt;
            SyncResult = syncResult;
        }

        [JsonProperty("finishedAt")] public string FinishedAt { get; }
        [JsonProperty("message")] public string Message { get; }
        [JsonProperty("operation")] public ApplicationStateRequestedOperation RequestedOperation { get; }
        [JsonProperty("phase")] public string Phase { get; }
        [JsonProperty("retryCount")] public string RetryCount { get; }
        [JsonProperty("startedAt")] public string StartedAt { get; }
        [JsonProperty("syncResult")] public ArgoApplicationSyncResult SyncResult { get; }
    }
}