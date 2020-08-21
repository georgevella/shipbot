using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quartz;

namespace Shipbot.JobScheduling
{
    public static class JobFactory
    {
        public static IJobDetail BuildJobWithData<TJob, TData>(string name, string group, TData data)
            where TJob : BaseJobWithData<TData>
            where TData : class
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            var jobData = new JobDataMap((IDictionary<string, object>) new Dictionary<string, object>()
            {
                {JobDataMapExtensions.JobDataKey, data}
            });
            
            return JobBuilder.Create<TJob>()
                .WithIdentity(name, group)
                .UsingJobData(jobData)
                .Build();
        }

        public static Task TriggerJobOnce(this IScheduler scheduler, IJobDetail jobDetail)
        {
            var trigger = TriggerBuilder.Create()
                .WithIdentity($"{jobDetail.Key.Name}-trigger-once", jobDetail.Key.Group)
                .StartNow()
                .ForJob(jobDetail)
                .Build();

            return scheduler.ScheduleJob(jobDetail, trigger);
        }

        public static Task TriggerJobOnce<TJob, TData>(this IScheduler scheduler, string name, string group, TData data)
            where TJob : BaseJobWithData<TData>
            where TData : class
        {
            var jobDetail = BuildJobWithData<TJob, TData>(name, group, data);

            return TriggerJobOnce(scheduler, jobDetail);
        }
    }

    public class ScheduleJobOptions
    {
        internal string Identity { get; set; }
        
        internal string Group { get; set; }
        
        
    }
}