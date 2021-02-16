using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    public class ApplicationSourceHelmFileParameter : BaseNameModel
    {
        protected bool Equals(ApplicationSourceHelmFileParameter other)
        {
            return base.Equals(other) && Path == other.Path;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationSourceHelmFileParameter) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Path != null ? Path.GetHashCode() : 0);
            }
        }

        [JsonConstructor]
        public ApplicationSourceHelmFileParameter(string name, string path) : base(name)
        {
            Path = path;
        }

        [JsonProperty("path")] public string Path { get; }
    }
}