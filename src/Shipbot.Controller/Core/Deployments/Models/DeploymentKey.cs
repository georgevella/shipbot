using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Deployments.Models
{
    public class DeploymentKey 
    {
        public DeploymentKey(Application application, Image image, string targetTag)
            : this(application, image.Repository, targetTag)
        {
            
        }
        public DeploymentKey(Application application, string containerRepository, string targetTag)
        {
            Application = application;
            ContainerRepository = containerRepository;
            TargetTag = targetTag;
        }
        
        protected bool Equals(DeploymentKey other)
        {
            return Equals(Application, other.Application) && Equals(ContainerRepository, other.ContainerRepository) && TargetTag == other.TargetTag;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DeploymentKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Application != null ? Application.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ContainerRepository != null ? ContainerRepository.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TargetTag != null ? TargetTag.GetHashCode() : 0);
                return hashCode;
            }
        }

        public Application Application { get; }
        
        public string ContainerRepository { get; }
        
        public string TargetTag { get; }
    }
}