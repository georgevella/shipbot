using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shipbot.Applications.Models;

namespace Shipbot.Applications
{
    public interface IApplicationImageInstanceService
    {
        Task<(bool available, string tag)> GetCurrentTagForPrimary(Application application, ApplicationImage applicationImage);
        
        Task<(bool available, string tag)> GetCurrentTag(Application application, ApplicationImage applicationImage, string name);
        Task SetCurrentTag(Application application, ApplicationImage applicationImage, string name, string tag);
        Task SetCurrentTagForPrimary(Application application, ApplicationImage applicationImage, string tag);
        IReadOnlyDictionary<ApplicationImage, string> GetAllCurrentTagsForPrimary(Application application);
        IEnumerable<string> GetAllInstanceIdsForApplication(Application application);
    }

    public class ApplicationImageInstanceService : IApplicationImageInstanceService
    {
        class Key
        {
            protected bool Equals(Key other)
            {
                return Application.Equals(other.Application) && Image.Equals(other.Image) && InstanceId == other.InstanceId;
            }

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Key) obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Application, Image, InstanceId);
            }

            public Key(Application application, ApplicationImage image, string name)
            {
                Application = application;
                Image = image;
                InstanceId = name;
            }

            public Application Application { get; }

            public ApplicationImage Image { get; }

            public string InstanceId { get; }
        }

        private static readonly ConcurrentDictionary<Key, string> CurrentTagStore = new ConcurrentDictionary<Key, string>();

        public Task<(bool available, string tag)> GetCurrentTagForPrimary(Application application, ApplicationImage applicationImage)
        {
            return GetCurrentTag(application, applicationImage, string.Empty);
        }

        public Task<(bool available, string tag)> GetCurrentTag(Application application, ApplicationImage applicationImage, string name)
        {
            var key = new Key(application, applicationImage, name);
            return Task.FromResult(CurrentTagStore.TryGetValue(key, out var currentTag) ? (true, currentTag) : (false, string.Empty));
        }

        public Task SetCurrentTagForPrimary(Application application, ApplicationImage applicationImage, string tag)
        {
            return SetCurrentTag(application, applicationImage, string.Empty, tag);
        }

        public IEnumerable<string> GetAllInstanceIdsForApplication(Application application)
        {
            return CurrentTagStore.Keys
                .Where(x => x.Application.Equals(application))
                .Select(key => key.InstanceId)
                .Distinct()
                .ToList();
        }

        public IReadOnlyDictionary<ApplicationImage, string> GetAllCurrentTagsForPrimary(Application application)
        {
            return CurrentTagStore.Keys.Where(x => x.Application.Equals(application) && x.InstanceId == string.Empty)
                .Select(key => (image: key.Image, tag: CurrentTagStore[key]) )
                .ToDictionary(x => x.image, x => x.tag);
        }

        public Task SetCurrentTag(Application application, ApplicationImage applicationImage, string name, string tag)
        {
            var key = new Key(application, applicationImage, name);
            CurrentTagStore.AddOrUpdate(key, tag, (k, s) => tag);
            
            return Task.CompletedTask;
        }
    }
}