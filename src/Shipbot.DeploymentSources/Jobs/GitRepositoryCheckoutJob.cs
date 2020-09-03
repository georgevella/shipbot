using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Shipbot.JobScheduling;

namespace Shipbot.Controller.Core.ApplicationSources.Jobs
{
    [DisallowConcurrentExecution]
    public class GitRepositoryCheckoutJob : BaseJobWithData<ApplicationSourceTrackingContext>
    {
        private readonly ILogger<GitRepositoryCheckoutJob> _log;
        private readonly IScheduler _scheduler;

        public GitRepositoryCheckoutJob(
            ILogger<GitRepositoryCheckoutJob> log,
            IScheduler scheduler)
        {
            _log = log;
            _scheduler = scheduler;
        }

        public override async Task Execute(ApplicationSourceTrackingContext data)
        {
            var repository = data.ApplicationSource.Repository;

            
            _log.LogInformation("Starting sync-job for {Repository} in {Path}",
                repository.Uri,
                data.GitRepositoryPath
            );

            var job = JobFactory.BuildJobWithData<GitRepositorySyncJob, ApplicationSourceTrackingContext>(
                $"gitwatch-{data.ApplicationName}", 
                Constants.SchedulerGroup, 
                data
                );

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"gitwatch-trig-{data.ApplicationName}", Constants.SchedulerGroup)
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
}