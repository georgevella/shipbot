using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.ApplicationSources
{
    public class ApplicationSourceService : IApplicationSourceService
    {
        private readonly ConcurrentDictionary<string, ApplicationSourceTrackingContext> _trackingContexts =
            new ConcurrentDictionary<string, ApplicationSourceTrackingContext>();
        
        private readonly ILogger<ApplicationSourceService> _log;
        private readonly IScheduler _scheduler;

        public ApplicationSourceService(ILogger<ApplicationSourceService> log, IScheduler scheduler)
        {
            _log = log;
            _scheduler = scheduler;
        }
        
        public async Task AddApplicationSource(Application application)
        {            
            var context = _trackingContexts.GetOrAdd(
                application.Name, 
                s => new ApplicationSourceTrackingContext(application)
            );

            var jobData = new JobDataMap((IDictionary<string, object>) new Dictionary<string, object>()
            {
                {"Context", context}
            });
            
            var job = JobBuilder.Create<GitRepositoryCheckoutJob>()
                .WithIdentity($"gitclone-{application.Name}", "gitrepowatcher")
                .UsingJobData(jobData)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"gitclone-trig-{application.Name}", "gitrepowatcher")
                .StartNow()
                .ForJob(job)
                .Build();

            await _scheduler.ScheduleJob(job, trigger);
        }

//        public IReadOnlyDictionary<Image, string> GetCurrentTags(Application application)
//        {
//            if (!_trackingContexts.TryGetValue(application.Name, out var context))
//            {
//                throw new InvalidOperationException("Application tracking context not found");
//            }
//
//            return new Dictionary<Image, string>(context.CurrentTags);
//        }
//
//        public async Task UpdateImageTag(Application application, Image image, string newTag)
//        {
//            if (!_trackingContexts.TryGetValue(application.Name, out var context))
//            {
//                throw new InvalidOperationException("Application tracking context not found");
//            }
//
//            context.CurrentTags[image] = newTag;
//        }
    }

    public interface IApplicationSourceService
    {
        Task AddApplicationSource(Application application);
    }
}