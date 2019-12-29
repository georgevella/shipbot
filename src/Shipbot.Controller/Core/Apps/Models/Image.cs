using System.Collections.Generic;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Apps.Models
{
    /// <summary>
    ///     Describes an image used within an environment.
    /// </summary>
    public class Image
    {
        private class ImageEqualityComparer : IEqualityComparer<Image>
        {
            public bool Equals(Image x, Image other)
            {
                if (x == null && other == null) return true;
                if (x == null) return false;
                if (other == null) return false;

                if (ReferenceEquals(x, other)) return true;
            
                return string.Equals(x.Repository, other.Repository) && 
                       Equals(x.TagProperty.Path, other.TagProperty.Path) &&
                       (x.TagProperty.ValueFormat == other.TagProperty.ValueFormat);
            }

            public int GetHashCode(Image obj)
            {
                unchecked
                {
                    var tagProperty = obj.TagProperty;
                    var tagPropertyHash = tagProperty != null
                        ? (((tagProperty.Path != null ? tagProperty.Path.GetHashCode() : 0) * 397) ^
                           (int) tagProperty.ValueFormat)
                        : 0;
                
                    var hashCode = (obj.Repository != null ? obj.Repository.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ tagPropertyHash;

                    return hashCode;
                }
            }
        }
        
        public static IEqualityComparer<Image> EqualityComparer { get; } = new ImageEqualityComparer();
        
        public string Repository { get; set; }

        public TagProperty TagProperty { get; set; }

        public Image(string repository, TagProperty tagProperty)
        {
            Repository = repository;
            TagProperty = tagProperty;
        }

        public Image()
        {
            
        }

        public override string ToString()
        {
            return $"{Repository}<{TagProperty.Path}>";
        }
    }

    public class TagProperty
    {
        public TagProperty(string path, TagPropertyValueFormat valueFormat)
        {
            Path = path;
            ValueFormat = valueFormat;
        }

        public string Path { get; set; }
        
        public TagPropertyValueFormat ValueFormat { get; set; }

        public TagProperty()
        {
            
        }
    }
    
    public enum TagPropertyValueFormat
    {
        TagOnly,
        TagAndRepository
    }
}