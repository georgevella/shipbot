using System.Collections.Generic;
using System.Threading.Tasks;
using Quartz;


namespace Shipbot.Controller.Core.Jobs
{
    public static class QuartzSchedulerExtensions
    {
        /// <summary>
        ///     Starts a job that is triggered only once.
        /// </summary>
        /// <param name="jobKey"></param>
        /// <param name="jobContext"></param>
        /// <param name="scheduler"></param>
        /// <typeparam name="TJob"></typeparam>
        /// <typeparam name="TJobContext"></typeparam>
        /// <returns></returns>
        public static async Task StartSingleThrowJob<TJob, TJobContext>(this IScheduler scheduler, JobKey jobKey, TJobContext jobContext)
            where TJob: BaseScheduledJob<TJobContext>
        {
            var jobData = new JobDataMap((IDictionary<string, object>) new Dictionary<string, object>()
            {
                {"Context", jobContext}
            });

            var job = JobBuilder.Create<TJob>()
                .WithIdentity(jobKey)
                .UsingJobData(jobData)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"trigger::{jobKey.Name}", jobKey.Group)
                .StartNow()
                .ForJob(job)
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
        
        public static async Task StartRecurringJob<TJob, TJobContext>(this IScheduler scheduler, JobKey jobKey, TJobContext jobContext, int interval)
            where TJob: BaseScheduledJob<TJobContext>
        {
            var jobData = new JobDataMap((IDictionary<string, object>) new Dictionary<string, object>()
            {
                {"Context", jobContext}
            });

            var job = JobBuilder.Create<TJob>()
                .WithIdentity(jobKey)
                .UsingJobData(jobData)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"trigger::{jobKey.Name}", jobKey.Group)
                .StartNow()
                .ForJob(job)
                .WithSimpleSchedule(x =>
                    x.WithIntervalInSeconds(interval)
                        .RepeatForever()
                )
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }
}