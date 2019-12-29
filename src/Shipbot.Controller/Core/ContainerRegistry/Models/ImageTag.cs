using System;
using System.Collections.Generic;

namespace Shipbot.Controller.Core.ContainerRegistry.Models
{
    public class ImageTag
    {
        private class ImageTagEqualityComparer : IEqualityComparer<ImageTag>
        {
            public bool Equals(ImageTag x, ImageTag y)
            {
                if (x == null && y == null) return true;
            
                if (x == null) return false;
                if (y == null) return false;
            
                if (ReferenceEquals(x, y)) return true;
            
                return (x.Repository == y.Repository) &&
                       (x.Tag == y.Tag) &&
                       (x.CreatedAt == y.CreatedAt);
            }
 
            public int GetHashCode(ImageTag obj)
            {
                return (obj.Repository.GetHashCode() ^ obj.Tag.GetHashCode()) ^ obj.CreatedAt.GetHashCode();
            }
        }
        
        public static IEqualityComparer<ImageTag> EqualityComparer { get; } = new ImageTagEqualityComparer();
        
        public string Repository { get; set; }
        public string Tag { get; set; }
        public DateTime CreatedAt { get; set; }

        public ImageTag()
        {
            
        }
        
        public ImageTag(string repository, string tag, DateTime createdAt)
        {
            Repository = repository;
            Tag = tag;
            CreatedAt = createdAt;
        }
    }
}