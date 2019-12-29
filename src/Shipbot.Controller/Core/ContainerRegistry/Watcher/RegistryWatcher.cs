using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Shipbot.Controller.Core.Apps.Models;

namespace Shipbot.Controller.Core.ContainerRegistry.Watcher
{
//    public class RegistryWatcher : IRegistryWatcher
//    {
//        private readonly ILogger<RegistryWatcher> _log;
//        private readonly IScheduler _scheduler;
//        private readonly ConcurrentDictionary<string, RegistryWatcherJobContext> _jobs 
//            = new ConcurrentDictionary<string,RegistryWatcherJobContext>();
//
//        public RegistryWatcher(ILogger<RegistryWatcher> log, IScheduler scheduler)
//        {
//            _log = log;
//            _scheduler = scheduler;
//        }
//
////        public async Task StartWatchingImageRepository(Application application)
////        {
////            _log.LogInformation("Adding application {name}, beginning watch of repositories", application.Name);
////            foreach (var env in application.Environments)
////            {
////                if (!env.Value.AutoDeploy)
////                    continue;
////
////                foreach (var image in env.Value.Images)
////                {
////                    var jobContext = new RegistryWatcherJobContext(image.Repository);
////
////                    if (_jobs.TryAdd(image.Repository, jobContext))
////                    {
////                        await _scheduler.ScheduleJob(jobContext.Job, jobContext.Trigger);
////                    }
////                }
////            }
////        }
//
//        private class RegistryWatcherJobContext
//        {
//            public IJobDetail Job { get; }
//            public ITrigger Trigger { get; }
//
//            public RegistryWatcherJobContext(string repository)
//            {
//                Job = JobBuilder.Create<RegistryWatcherJob>()
//                    .WithIdentity($"rwatcher-{repository}", "containerrepowatcher")
//                    .UsingJobData("ImageRepository", repository)
//                    .Build();
//
//                Trigger = TriggerBuilder.Create()
//                    .WithIdentity($"rwatcher-trig-{repository}", "containerrepowatcher")
//                    .StartNow()
//                    .WithSimpleSchedule(x => x
//                        .WithIntervalInSeconds(10)
//                        .RepeatForever()
//                    )
//                    .ForJob(Job)
//                    .Build();
//            }
//        }
//    }
//
//    public interface IRegistryWatcher
//    {
//        //Task StartWatchingImageRepository(Application application);
//    }
}