using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    public class BaseNameValueModel : BaseNameModel
    {
        protected bool Equals(BaseNameValueModel other)
        {
            return base.Equals(other) && Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BaseNameValueModel) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }

        [JsonConstructor]
        public BaseNameValueModel(string name, string value) : base(name)
        {
            Value = value;
        }

        [JsonProperty("value")] public string Value { get; }
    }
}