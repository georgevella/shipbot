using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Shipbot.Controller.Core.Models;
using Image = Shipbot.Controller.Core.Models.Image;

namespace Shipbot.Controller.Core.Registry.Watcher
{
    public class RegistryWatcher : IRegistryWatcher
    {
        private readonly ILogger<RegistryWatcher> _log;
        private readonly IScheduler _scheduler;
        private readonly RegistryClientPool _registryClientPool;

        public RegistryWatcher(ILogger<RegistryWatcher> log, IScheduler scheduler,  RegistryClientPool registryClientPool)
        {
            _log = log;
            _scheduler = scheduler;
            _registryClientPool = registryClientPool;
        }

        public async Task StartWatchingImageRepository(Application application)
        {
            _log.LogInformation("Adding application {name}, beginning watch of repositories", application.Name);
            for (int i=0; i<application.Images.Count; i++)
            {
                var image = application.Images[i];
                
                var job = JobBuilder.Create<RegistryWatcherJob>()
                    .WithIdentity($"rwatcher-{application.Name}-{image.Repository}", "containerrepowatcher")
                    .UsingJobData("ImageRepository", image.Repository)
                    .UsingJobData("Application", application.Name)
                    .UsingJobData("ImageIndex", i)
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity($"rwatcher-trig-{application.Name}-{image.Repository}", "containerrepowatcher")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(10)
                        .RepeatForever()
                    )
                    .ForJob(job)
                    .Build();
                
                await _scheduler.ScheduleJob(job, trigger);
            }
        }

        private class ImageRegistryWatcherContext
        {
            public Image Image { get; }
            
            public IJob Job { get; }
        }
    }

    public interface IRegistryWatcher
    {
        Task StartWatchingImageRepository(Application application);
    }
}