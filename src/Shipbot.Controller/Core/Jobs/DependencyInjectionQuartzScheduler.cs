using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Spi;

namespace Shipbot.Controller.Core.Jobs
{
    public class DependencyInjectionQuartzScheduler : IScheduler
    {
        private readonly IScheduler _schedulerImplementation;

        public DependencyInjectionQuartzScheduler(ISchedulerFactory schedulerFactory, IJobFactory jobFactory)
        {
            var scheduler = schedulerFactory.GetScheduler().Result;
            scheduler.JobFactory = jobFactory;
            scheduler.Start();
            
            _schedulerImplementation = scheduler;
        }
        
        public Task<bool> IsJobGroupPaused(string groupName, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.IsJobGroupPaused(groupName, cancellationToken);
        }

        public Task<bool> IsTriggerGroupPaused(string groupName, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.IsTriggerGroupPaused(groupName, cancellationToken);
        }

        public Task<SchedulerMetaData> GetMetaData(CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.GetMetaData(cancellationToken);
        }

        public Task<IReadOnlyCollection<IJobExecutionContext>> GetCurrentlyExecutingJobs(CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.GetCurrentlyExecutingJobs(cancellationToken);
        }

        public Task<IReadOnlyCollection<string>> GetJobGroupNames(CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.GetJobGroupNames(cancellationToken);
        }

        public Task<IReadOnlyCollection<string>> GetTriggerGroupNames(CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.GetTriggerGroupNames(cancellationToken);
        }

        public Task<IReadOnlyCollection<string>> GetPausedTriggerGroups(CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.GetPausedTriggerGroups(cancellationToken);
        }

        public Task Start(CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.Start(cancellationToken);
        }

        public Task StartDelayed(TimeSpan delay, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.StartDelayed(delay, cancellationToken);
        }

        public Task Standby(CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.Standby(cancellationToken);
        }

        public Task Shutdown(CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.Shutdown(cancellationToken);
        }

        public Task Shutdown(bool waitForJobsToComplete, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.Shutdown(waitForJobsToComplete, cancellationToken);
        }

        public Task<DateTimeOffset> ScheduleJob(IJobDetail jobDetail, ITrigger trigger, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.ScheduleJob(jobDetail, trigger, cancellationToken);
        }

        public Task<DateTimeOffset> ScheduleJob(ITrigger trigger, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.ScheduleJob(trigger, cancellationToken);
        }

        public Task ScheduleJobs(IReadOnlyDictionary<IJobDetail, IReadOnlyCollection<ITrigger>> triggersAndJobs, bool replace,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.ScheduleJobs(triggersAndJobs, replace, cancellationToken);
        }

        public Task ScheduleJob(IJobDetail jobDetail, IReadOnlyCollection<ITrigger> triggersForJob, bool replace,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.ScheduleJob(jobDetail, triggersForJob, replace, cancellationToken);
        }

        public Task<bool> UnscheduleJob(TriggerKey triggerKey, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.UnscheduleJob(triggerKey, cancellationToken);
        }

        public Task<bool> UnscheduleJobs(IReadOnlyCollection<TriggerKey> triggerKeys, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.UnscheduleJobs(triggerKeys, cancellationToken);
        }

        public Task<DateTimeOffset?> RescheduleJob(TriggerKey triggerKey, ITrigger newTrigger,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.RescheduleJob(triggerKey, newTrigger, cancellationToken);
        }

        public Task AddJob(IJobDetail jobDetail, bool replace, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.AddJob(jobDetail, replace, cancellationToken);
        }

        public Task AddJob(IJobDetail jobDetail, bool replace, bool storeNonDurableWhileAwaitingScheduling,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.AddJob(jobDetail, replace, storeNonDurableWhileAwaitingScheduling, cancellationToken);
        }

        public Task<bool> DeleteJob(JobKey jobKey, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.DeleteJob(jobKey, cancellationToken);
        }

        public Task<bool> DeleteJobs(IReadOnlyCollection<JobKey> jobKeys, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.DeleteJobs(jobKeys, cancellationToken);
        }

        public Task TriggerJob(JobKey jobKey, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.TriggerJob(jobKey, cancellationToken);
        }

        public Task TriggerJob(JobKey jobKey, JobDataMap data, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.TriggerJob(jobKey, data, cancellationToken);
        }

        public Task PauseJob(JobKey jobKey, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.PauseJob(jobKey, cancellationToken);
        }

        public Task PauseJobs(GroupMatcher<JobKey> matcher, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.PauseJobs(matcher, cancellationToken);
        }

        public Task PauseTrigger(TriggerKey triggerKey, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.PauseTrigger(triggerKey, cancellationToken);
        }

        public Task PauseTriggers(GroupMatcher<TriggerKey> matcher, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.PauseTriggers(matcher, cancellationToken);
        }

        public Task ResumeJob(JobKey jobKey, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.ResumeJob(jobKey, cancellationToken);
        }

        public Task ResumeJobs(GroupMatcher<JobKey> matcher, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.ResumeJobs(matcher, cancellationToken);
        }

        public Task ResumeTrigger(TriggerKey triggerKey, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.ResumeTrigger(triggerKey, cancellationToken);
        }

        public Task ResumeTriggers(GroupMatcher<TriggerKey> matcher, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.ResumeTriggers(matcher, cancellationToken);
        }

        public Task PauseAll(CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.PauseAll(cancellationToken);
        }

        public Task ResumeAll(CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.ResumeAll(cancellationToken);
        }

        public Task<IReadOnlyCollection<JobKey>> GetJobKeys(GroupMatcher<JobKey> matcher, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.GetJobKeys(matcher, cancellationToken);
        }

        public Task<IReadOnlyCollection<ITrigger>> GetTriggersOfJob(JobKey jobKey, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.GetTriggersOfJob(jobKey, cancellationToken);
        }

        public Task<IReadOnlyCollection<TriggerKey>> GetTriggerKeys(GroupMatcher<TriggerKey> matcher, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.GetTriggerKeys(matcher, cancellationToken);
        }

        public Task<IJobDetail> GetJobDetail(JobKey jobKey, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.GetJobDetail(jobKey, cancellationToken);
        }

        public Task<ITrigger> GetTrigger(TriggerKey triggerKey, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.GetTrigger(triggerKey, cancellationToken);
        }

        public Task<TriggerState> GetTriggerState(TriggerKey triggerKey, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.GetTriggerState(triggerKey, cancellationToken);
        }

        public Task AddCalendar(string calName, ICalendar calendar, bool replace, bool updateTriggers,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.AddCalendar(calName, calendar, replace, updateTriggers, cancellationToken);
        }

        public Task<bool> DeleteCalendar(string calName, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.DeleteCalendar(calName, cancellationToken);
        }

        public Task<ICalendar> GetCalendar(string calName, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.GetCalendar(calName, cancellationToken);
        }

        public Task<IReadOnlyCollection<string>> GetCalendarNames(CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.GetCalendarNames(cancellationToken);
        }

        public Task<bool> Interrupt(JobKey jobKey, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.Interrupt(jobKey, cancellationToken);
        }

        public Task<bool> Interrupt(string fireInstanceId, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.Interrupt(fireInstanceId, cancellationToken);
        }

        public Task<bool> CheckExists(JobKey jobKey, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.CheckExists(jobKey, cancellationToken);
        }

        public Task<bool> CheckExists(TriggerKey triggerKey, CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.CheckExists(triggerKey, cancellationToken);
        }

        public Task Clear(CancellationToken cancellationToken = new CancellationToken())
        {
            return _schedulerImplementation.Clear(cancellationToken);
        }

        public string SchedulerName => _schedulerImplementation.SchedulerName;

        public string SchedulerInstanceId => _schedulerImplementation.SchedulerInstanceId;

        public SchedulerContext Context => _schedulerImplementation.Context;

        public bool InStandbyMode => _schedulerImplementation.InStandbyMode;

        public bool IsShutdown => _schedulerImplementation.IsShutdown;

        public IJobFactory JobFactory
        {
            set => _schedulerImplementation.JobFactory = value;
        }

        public IListenerManager ListenerManager => _schedulerImplementation.ListenerManager;

        public bool IsStarted => _schedulerImplementation.IsStarted;
    }
}