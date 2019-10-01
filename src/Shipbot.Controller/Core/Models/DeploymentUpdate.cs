using System;

namespace Shipbot.Controller.Core.Models
{
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

        public Application Application { get; }
        
        public ApplicationEnvironment Environment { get; }
        public Image Image { get; }
        public string CurrentTag { get; }

        public string TargetTag { get; }

        public DeploymentUpdate(
            Application application,
            ApplicationEnvironment environment,
            Image image,
            string currentTag,
            string targetTag
            )
        {
            Application = application;
            Environment = environment;
            Image = image;
            CurrentTag = currentTag;
            TargetTag = targetTag;
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
        Failed
    }
}