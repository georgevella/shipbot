using System.Collections.Generic;
using System.Linq;
using Shipbot.Controller.Core.ContainerRegistry.Models;

namespace Shipbot.Controller.Core.ContainerRegistry.Watcher
{
    public class RegistryWatcherStorage : IRegistryWatcherStorage
    {
        private readonly object _lock = new object();
        private readonly Dictionary<string, HashSet<ImageTag>> _containerRepositoryTags = new Dictionary<string, HashSet<ImageTag>>();
        
        /// <summary>
        ///     Adds one or more tags to the list of container repository tags, and returns any new tags detected. 
        /// </summary>
        /// <param name="containerRepository"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public IEnumerable<ImageTag> AddOrUpdateImageTags(string containerRepository, IEnumerable<ImageTag> tags)
        {
            lock (_lock)
            {
                if (!_containerRepositoryTags.TryGetValue(containerRepository, out var tagList))
                {
                    tagList = new HashSet<ImageTag>();
                    _containerRepositoryTags.Add(containerRepository, tagList);
                }

                var newTags = tags.Except(tagList).ToArray();
                foreach (var imageTag in newTags)
                {
                    tagList.Add(imageTag);
                }
                return newTags;
            }
        }

        public IEnumerable<ImageTag> GetRepositoryTags(string containerRepository)
        {
            lock (_lock)
            {
                if (_containerRepositoryTags.TryGetValue(containerRepository, out var imageTags))
                {
                    return imageTags.ToList();
                }

                return Enumerable.Empty<ImageTag>();
            }
        }
    }

    public interface IRegistryWatcherStorage
    {
        /// <summary>
        ///     Adds one or more tags to the list of container repository tags, and returns any new tags detected. 
        /// </summary>
        /// <param name="containerRepository"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        IEnumerable<ImageTag> AddOrUpdateImageTags(string containerRepository, IEnumerable<ImageTag> tags);

        IEnumerable<ImageTag> GetRepositoryTags(string containerRepository);
    }
}