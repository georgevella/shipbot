using System.Collections.Generic;
using Shipbot.Controller.Core.ContainerRegistry.Models;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Apps.Models
{
    public class DeployedImageTag
    {
        internal class DeployedImageTagEqualityComparer : IEqualityComparer<DeployedImageTag>
        {
            public bool Equals(DeployedImageTag x, DeployedImageTag y)
            {
                if (x == null && y == null) return true;
                if (x == null) return false;
                if (y == null) return false;

                if (ReferenceEquals(x, y)) return true;
            
                return Image.EqualityComparer.Equals(x.Image, y.Image);
            }

            public int GetHashCode(DeployedImageTag obj)
            {
                return Image.EqualityComparer.GetHashCode(obj.Image);
            }
        }
        
        public static IEqualityComparer<DeployedImageTag> EqualityComparer { get; } = new DeployedImageTagEqualityComparer();
        
        public string Tag { get; set; }
        
        public Image Image { get; set; }

        public static implicit operator DeployedImageTag(Image image)
        {
            return new DeployedImageTag()
            {
                Image = image
            };
        }

        public override string ToString()
        {
            return $"{Image}:{Tag}";
        }
    }
}