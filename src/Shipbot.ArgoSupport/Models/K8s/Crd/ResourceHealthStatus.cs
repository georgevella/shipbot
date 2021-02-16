using System.Diagnostics;
using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    [DebuggerDisplay("Heath: {Status}")]
    public class ResourceHealthStatus
    {
        protected bool Equals(ResourceHealthStatus other)
        {
            return Status == other.Status && Message == other.Message;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ResourceHealthStatus) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Status * 397) ^ (Message != null ? Message.GetHashCode() : 0);
            }
        }

        [JsonConstructor]
        public ResourceHealthStatus(HealthStatusCode status, string message)
        {
            Status = status;
            Message = message;
        }

        [JsonProperty("status")] public HealthStatusCode Status { get; }
        [JsonProperty("message")] public string Message { get; }
    }
}