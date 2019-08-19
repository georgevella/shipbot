using System;

namespace Shipbot.Controller.Core.Models
{
    public class DeploymentUpdate
    {
        protected bool Equals(DeploymentUpdate other)
        {
            return Image.Equals(other.Image) && string.Equals(Tag, other.Tag, StringComparison.OrdinalIgnoreCase);
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
                return (Image.GetHashCode() * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Tag);
            }
        }

        public Image Image { get; }
        
        public string Tag { get; }
        
        public DeploymentUpdateStatus Status { get; set; }

        public DeploymentUpdate(Image image, string tag)
        {
            Image = image;
            Tag = tag;
            Status = DeploymentUpdateStatus.Pending;
        }
    }

    public enum DeploymentUpdateStatus
    {
        Pending,
        UpdatingManifests,
        Synchronizing,
        Synchronized
    }
}