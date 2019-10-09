using System;
using System.Linq;

namespace Shipbot.Controller.Core.Models
{
    /// <summary>
    ///     Describes a deployment change to execute by one of the deployment source updaters.
    /// </summary>
    public class DeploymentUpdate
    {
        protected bool Equals(DeploymentUpdate other)
        {
            return Equals(Application, other.Application) && Equals(Environment, other.Environment) && Equals(Image, other.Image) && CurrentTag == other.CurrentTag && TargetTag == other.TargetTag;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DeploymentUpdate) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Application != null ? Application.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Environment != null ? Environment.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Image != null ? Image.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CurrentTag != null ? CurrentTag.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TargetTag != null ? TargetTag.GetHashCode() : 0);
                return hashCode;
            }
        }
        
        /// <summary>
        ///     Deployment update that triggered the current deployment.
        /// </summary>
        public DeploymentUpdate SourceDeploymentUpdate { get; }

        public Application Application { get; }
        public ApplicationEnvironment Environment { get; }
        public Image Image { get; }
        public string CurrentTag { get; }
        public string TargetTag { get; }
        public bool IsPromotable => Environment.PromotionEnvironments.Any();

        public bool IsTriggeredByPromotion => SourceDeploymentUpdate != null;

        public DeploymentUpdate(
            Application application,
            ApplicationEnvironment environment,
            Image image,
            string currentTag,
            string targetTag,
            DeploymentUpdate sourceDeploymentUpdate = null
            )
        {
            Application = application;
            Environment = environment;
            Image = image;
            CurrentTag = currentTag;
            TargetTag = targetTag;
            SourceDeploymentUpdate = sourceDeploymentUpdate;
        }
        
        
    }

    public enum DeploymentUpdateStatus
    {
        Pending,
        Starting,
        UpdatingManifests,
        Synchronizing,
        Synchronized,
        Complete,
        Promoting,
        Promoted,
        Failed
    }
}