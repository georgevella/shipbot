using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    public class ArgoApplicationSyncPolicy
    {
        protected bool Equals(ArgoApplicationSyncPolicy other)
        {
            return Equals(Automated, other.Automated);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ArgoApplicationSyncPolicy) obj);
        }

        public override int GetHashCode()
        {
            return (Automated != null ? Automated.GetHashCode() : 0);
        }

        public ArgoApplicationSyncPolicy(ArgoApplicationSyncPolicyAutomation automated)
        {
            Automated = automated;
        }

        [JsonProperty("automated")]
        public ArgoApplicationSyncPolicyAutomation Automated { get; }
    }
}