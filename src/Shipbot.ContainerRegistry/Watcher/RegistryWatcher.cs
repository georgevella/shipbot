using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Shipbot.Contracts;
using Shipbot.JobScheduling;
using Shipbot.Models;

namespace Shipbot.ContainerRegistry.Watcher
{
    public class RegistryWatcher : IRegistryWatcher
    {
        private readonly ILogger<RegistryWatcher> _log;
        private readonly IScheduler _scheduler;
        private readonly ConcurrentDictionary<RegistryWatcherKey, string> _jobs = new ConcurrentDictionary<RegistryWatcherKey,string>();

        public RegistryWatcher(ILogger<RegistryWatcher> log, IScheduler scheduler)
        {
            _log = log;
            _scheduler = scheduler;
        }

        public async Task StartWatchingImageRepository(Application application)
        {
            _log.LogInformation("Adding application {name}, beginning watch of repositories", application.Name);
            for (var imageIndex=0; imageIndex<application.Images.Count; imageIndex++)
            {
                var image = application.Images[imageIndex];

                var key = new RegistryWatcherKey(application, image);
                var jobKey = $"rwatcher-{application.Name}-{image.Repository}-{imageIndex}";

                if (_jobs.TryAdd(key, jobKey))
                {
                    await _scheduler.StartRecurringJob<ContainerRegistryPollingJob, ContainerRegistryPollingData>(
                        jobKey, 
                        "containerrepowatcher", 
                        new ContainerRegistryPollingData(image.Repository, application.Name, imageIndex), 
                        TimeSpan.FromSeconds(10) 
                    );
                }
            }
        }

        private class RegistryWatcherKey
        {
            protected bool Equals(RegistryWatcherKey other)
            {
                return _application.Equals(other._application) && _image.Equals(other._image);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((RegistryWatcherKey) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_application.GetHashCode() * 397) ^ _image.GetHashCode();
                }
            }

            private readonly Application _application;
            private readonly ApplicationImage _image;

            public RegistryWatcherKey(Application application, ApplicationImage image)
            {
                _application = application;
                _image = image;
            }
        }
    }
}