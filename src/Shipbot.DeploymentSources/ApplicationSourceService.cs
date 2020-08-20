using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Shipbot.Contracts;
using Shipbot.Controller.Core.ApplicationSources.Jobs;
using Shipbot.Models;

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
            
            var jobKey = new JobKey($"gitclone-{application.Name}", "gitrepowatcher");
            
            var job = JobBuilder.Create<GitRepositoryCheckoutJob>()
                .WithIdentity(jobKey)
                .UsingJobData(jobData)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"gitclone-trig-{application.Name}", "gitrepowatcher")
                .StartNow()
                .ForJob(job)
                .Build();

            await _scheduler.ScheduleJob(job, trigger);
        }

        public async Task StartDeploymentUpdateJob(DeploymentUpdate deploymentUpdate)
        {
            var jobkey = new JobKey($"gitwatch-{deploymentUpdate.Application.Name}", "gitrepowatcher");

            var data = new JobDataMap
            {
                ["DeploymentUpdate"] = deploymentUpdate
            };

            await _scheduler.TriggerJob(jobkey, data, CancellationToken.None);
        }
    }
}