using System.Collections.Generic;
using Newtonsoft.Json;
using Shipbot.Controller.Core.Apps.Models;

namespace Shipbot.Controller.Core.Apps.GrainState
{
    /// <summary>
    ///     Describes an image used within an environment.
    /// </summary>
    public class ApplicationEnvironmentImageMetadata
    {
        private class ImageEqualityComparer : IEqualityComparer<ApplicationEnvironmentImageMetadata>
        {
            public bool Equals(ApplicationEnvironmentImageMetadata x, ApplicationEnvironmentImageMetadata other)
            {
                if (x == null && other == null) return true;
                if (x == null) return false;
                if (other == null) return false;

                if (ReferenceEquals(x, other)) return true;
            
                return string.Equals(x.Repository, other.Repository) && 
                       Equals(x.ImageTagValuePath, other.ImageTagValuePath) &&
                       (x.ImageTagValueFormat == other.ImageTagValueFormat);
            }

            public int GetHashCode(ApplicationEnvironmentImageMetadata obj)
            {
                unchecked
                {
                    var hashCode = (obj.Repository != null ? obj.Repository.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ obj.ImageTagValuePath.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.ImageTagValueFormat.GetHashCode();
                    return hashCode;
                }
            }
        }
        
        public static IEqualityComparer<ApplicationEnvironmentImageMetadata> EqualityComparer { get; } = new ImageEqualityComparer();
        
        public string Repository { get; }
        
        public string CurrentTag { get; set; }
        
        public string ImageTagValuePath { get; }
        
        public TagPropertyValueFormat ImageTagValueFormat { get; }
        
        public ImageUpdatePolicy Policy { get; }

        [JsonConstructor]
        public ApplicationEnvironmentImageMetadata(
            string repository,
            string currentTag,
            string imageTagValuePath, 
            TagPropertyValueFormat imageTagValueFormat,
            ImageUpdatePolicy policy)
        {
            Repository = repository;
            Policy = policy;
            CurrentTag = currentTag;
            ImageTagValuePath = imageTagValuePath;
            ImageTagValueFormat = imageTagValueFormat;
        }

        public override string ToString()
        {
            return $"{Repository}<{ImageTagValuePath}>";
        }

        public static implicit operator ApplicationEnvironmentImageKey(ApplicationEnvironmentImageMetadata metadata)
        {
            return new ApplicationEnvironmentImageKey(metadata.Repository, metadata.ImageTagValuePath);
        } 
    }

    public enum TagPropertyValueFormat
    {
        TagOnly,
        TagAndRepository
    }
}