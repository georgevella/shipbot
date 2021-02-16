using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    public class ApplicationSourceHelmParameter : BaseNameValueModel
    {
        protected bool Equals(ApplicationSourceHelmParameter other)
        {
            return base.Equals(other) && ForceString == other.ForceString;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationSourceHelmParameter) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ ForceString.GetHashCode();
            }
        }

        [JsonConstructor]
        public ApplicationSourceHelmParameter(string name, string value, bool forceString) : base(name, value)
        {
            ForceString = forceString;
        }

        [JsonProperty("forceString")] public bool ForceString { get; }
    }
}