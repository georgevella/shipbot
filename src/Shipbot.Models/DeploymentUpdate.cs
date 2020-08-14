using System;

namespace Shipbot.Models
{
    public class DeploymentUpdate
    {
        protected bool Equals(DeploymentUpdate other)
        {
            return Application.Equals(other.Application) && 
                   Image.Equals(other.Image) && 
                   string.Equals(TargetTag, other.TargetTag, StringComparison.OrdinalIgnoreCase);
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
                return (Application.GetHashCode() * 984) ^ 
                       (Image.GetHashCode() * 397) ^ 
                       StringComparer.OrdinalIgnoreCase.GetHashCode(TargetTag) ^
                       StringComparer.OrdinalIgnoreCase.GetHashCode(CurrentTag);
            }
        }

        public Guid Id { get; }
        public Application Application { get; }
        public Image Image { get; }
        public string CurrentTag { get; }

        public string TargetTag { get; }

        public DeploymentUpdate(
            Guid id,
            Application application, 
            Image image, 
            string currentTag,
            string targetTag
            )
        {
            Id = id;
            Application = application;
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