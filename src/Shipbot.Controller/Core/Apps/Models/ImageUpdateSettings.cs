using System.Collections.Generic;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Apps.Models
{
    public class ImageUpdateSettings
    {
        private sealed class ImageUpdateSettingsEqualityComparer : IEqualityComparer<ImageUpdateSettings>
        {
            public bool Equals(ImageUpdateSettings x, ImageUpdateSettings y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return Image.EqualityComparer.Equals(x.Image, y.Image);
            }

            public int GetHashCode(ImageUpdateSettings obj)
            {
                return (obj.Image != null ? Image.EqualityComparer.GetHashCode(obj.Image) : 0);
            }
        }

        public static IEqualityComparer<ImageUpdateSettings> EqualityComparer { get; } = new ImageUpdateSettingsEqualityComparer();

        public bool AutoUpdate { get; set; }
        
        public ImageUpdatePolicy Policy { get; set; }
        
        public Image Image { get; set; }
    }
}