using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    public class BaseNameModel
    {
        protected bool Equals(BaseNameModel other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BaseNameModel) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        [JsonConstructor]
        public BaseNameModel(string name)
        {
            Name = name;
        }

        [JsonProperty("name")] public string Name { get; }
    }
}