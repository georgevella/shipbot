using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    public class ApplicationSourceDirectory
    {
        protected bool Equals(ApplicationSourceDirectory other)
        {
            return Exclude == other.Exclude && Include == other.Include && Recurse == other.Recurse;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationSourceDirectory) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Exclude != null ? Exclude.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Include != null ? Include.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Recurse.GetHashCode();
                return hashCode;
            }
        }

        [JsonConstructor]
        public ApplicationSourceDirectory(string exclude, string include, bool recurse, object jsonnet)
        {
            Exclude = exclude;
            Include = include;
            Recurse = recurse;
            Jsonnet = jsonnet;
        }

        [JsonProperty("exclude")] public string Exclude { get; }
        [JsonProperty("include")] public string Include { get; }
        [JsonProperty("recurse")] public bool Recurse { get; }
        [JsonProperty("jsonnet")] public object Jsonnet { get; }
    }
}