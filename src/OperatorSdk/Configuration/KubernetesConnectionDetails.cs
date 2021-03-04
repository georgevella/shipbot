namespace OperatorSdk.Configuration
{
    public class KubernetesConnectionDetails
    {
        protected bool Equals(KubernetesConnectionDetails other)
        {
            return Mode == other.Mode && string.Equals(Name, other.Name) && Equals(Eks, other.Eks) && Equals(File, other.File);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((KubernetesConnectionDetails) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Mode;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Eks != null ? Eks.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (File != null ? File.GetHashCode() : 0);
                return hashCode;
            }
        }

        public KubernetesConnectionMode Mode { get; set; }

        public string Name { get; set; }

        public EksConnectionDetails Eks { get; set; }

        public KubeConfigFile File { get; set; }
    }
}