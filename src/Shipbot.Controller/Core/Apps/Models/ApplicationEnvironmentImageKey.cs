using System.Collections.Generic;
using Newtonsoft.Json;

namespace Shipbot.Controller.Core.Apps.Models
{
    public class ApplicationEnvironmentImageKey
    {
        private sealed class RepositoryImageTagValuePathEqualityComparer : IEqualityComparer<ApplicationEnvironmentImageKey>
        {
            public bool Equals(ApplicationEnvironmentImageKey x, ApplicationEnvironmentImageKey y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Repository == y.Repository && x.ImageTagValuePath == y.ImageTagValuePath;
            }

            public int GetHashCode(ApplicationEnvironmentImageKey obj)
            {
                unchecked
                {
                    return (obj.Repository.GetHashCode() * 397) ^ obj.ImageTagValuePath.GetHashCode();
                }
            }
        }

        public static IEqualityComparer<ApplicationEnvironmentImageKey> EqualityComparer { get; } = new RepositoryImageTagValuePathEqualityComparer();

        [JsonConstructor]
        public ApplicationEnvironmentImageKey(string repository, string imageTagValuePath)
        {
            Repository = repository;
            ImageTagValuePath = imageTagValuePath;
        }

        /// <summary>
        ///     URI of repository containing all images
        /// </summary>
        public string Repository { get; }

        /// <summary>
        ///     Path within the deployment source where we can find and store the image tag.
        /// </summary>
        public string ImageTagValuePath { get; }
    }
}