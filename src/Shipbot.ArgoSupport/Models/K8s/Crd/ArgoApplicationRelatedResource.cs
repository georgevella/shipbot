using System.Diagnostics;
using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    [DebuggerDisplay("Name: {Name}, Kind: {Kind}")]
    public class ArgoApplicationRelatedResource : BaseNameModel
    {
        protected bool Equals(ArgoApplicationRelatedResource other)
        {
            return base.Equals(other) && 
                   Group == other.Group &&
                   Kind == other.Kind && 
                   Namespace == other.Namespace;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ArgoApplicationRelatedResource) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Group != null ? Group.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Kind != null ? Kind.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Namespace != null ? Namespace.GetHashCode() : 0);
                return hashCode;
            }
        }

        [JsonConstructor]
        public ArgoApplicationRelatedResource(
            string name, 
            string @group, 
            string kind, 
            string @namespace
        ) : base(name)
        {
            Group = @group;
            Kind = kind;
            Namespace = @namespace;
        }

        [JsonProperty("group")] public string Group { get; }
        [JsonProperty("kind")] public string Kind { get; }
        [JsonProperty("namespace")] public string Namespace { get; }
    }
}