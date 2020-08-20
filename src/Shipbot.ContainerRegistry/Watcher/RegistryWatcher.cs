using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Shipbot.Contracts;
using Shipbot.Models;
using Image = Shipbot.Models.Image;

namespace Shipbot.Controller.Core.Registry.Watcher
{
    public class RegistryWatcher : IRegistryWatcher
    {
        private readonly ILogger<RegistryWatcher> _log;
        private readonly IScheduler _scheduler;
        private readonly ConcurrentDictionary<RegistryWatcherKey, RegistryWatcherJobContext> _jobs = new ConcurrentDictionary<RegistryWatcherKey,RegistryWatcherJobContext>();

        public RegistryWatcher(ILogger<RegistryWatcher> log, IScheduler scheduler)
        {
            _log = log;
            _scheduler = scheduler;
        }

        public async Task StartWatchingImageRepository(Application application)
        {
            _log.LogInformation("Adding application {name}, beginning watch of repositories", application.Name);
            for (var i=0; i<application.Images.Count; i++)
            {
                var image = application.Images[i];

                var key = new RegistryWatcherKey(application, image);
                var jobContext = new RegistryWatcherJobContext(application, image, i);

                if (_jobs.TryAdd(key, jobContext))
                {
                    await _scheduler.ScheduleJob(jobContext.Job, jobContext.Trigger);    
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
            private readonly Image _image;

            public RegistryWatcherKey(Application application, Image image)
            {
                _application = application;
                _image = image;
            }
        }

        private class RegistryWatcherJobContext
        {
            public IJobDetail Job { get; }
            public ITrigger Trigger { get; }

            public RegistryWatcherJobContext(Application application, Image image, int imageIndex)
            {
                Job = JobBuilder.Create<RegistryWatcherJob>()
                    .WithIdentity($"rwatcher-{application.Name}-{image.Repository}-{imageIndex}", "containerrepowatcher")
                    .UsingJobData("ImageRepository", image.Repository)
                    .UsingJobData("Application", application.Name)
                    .UsingJobData("ImageIndex", imageIndex)
                    .Build();

                Trigger = TriggerBuilder.Create()
                    .WithIdentity($"rwatcher-trig-{application.Name}-{image.Repository}-{imageIndex}", "containerrepowatcher")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(10)
                        .RepeatForever()
                    )
                    .ForJob(Job)
                    .Build();
            }
        }
    }
}