using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    public class ApplicationDestination
    {
        public ApplicationDestination(string @namespace, string server, string name)
        {
            Namespace = @namespace;
            Server = server;
            Name = name;
        }

        protected bool Equals(ApplicationDestination other)
        {
            return Namespace == other.Namespace && Server == other.Server && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationDestination) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Namespace != null ? Namespace.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Server != null ? Server.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                return hashCode;
            }
        }

        [JsonProperty("namespace")] public string Namespace { get; }
        [JsonProperty("server")] public string Server { get; }
        [JsonProperty("name")] public string Name { get; }
    }
}