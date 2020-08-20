using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Spi;

namespace Shipbot.JobScheduling
{
    public class DependencyInjectionQuartzJobFactory : IJobFactory
    {
        private readonly IServiceProvider _container;

        public DependencyInjectionQuartzJobFactory(IServiceProvider container)
        {
            _container = container;
        }
        
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var jobType = bundle.JobDetail.JobType;
            
            var loggerType = typeof(ILogger<>).MakeGenericType(jobType);
            var wrapperType = typeof(JobWrapper<>).MakeGenericType(jobType);
            
            var scope = _container.CreateScope();
            var job = scope.ServiceProvider.GetService(jobType) as IJob;
            var logger = scope.ServiceProvider.GetService(loggerType);

            if (job == null)
            {
                throw new InvalidOperationException($"Job '{jobType.ToString()}' not resolved");
            }

            return (IJob) Activator.CreateInstance(wrapperType, logger, scope, job, bundle.JobDetail);
        }

        public void ReturnJob(IJob job)
        {
            if (job is IScopedJob scopedJob)
            {
                scopedJob.Scope.Dispose();
            }
        }

        private class JobWrapper<T> : IJob, IScopedJob
            where T: IJob
        {
            private readonly ILogger<T> _logger;
            private readonly T _job;
            private readonly IJobDetail _jobDetail;
            private readonly IServiceScope _scope;
            
            IServiceScope IScopedJob.Scope => _scope;

            public JobWrapper(ILogger<T> logger, IServiceScope scope, T job, IJobDetail jobDetail)
            {
                _logger = logger;
                _scope = scope;
                _job = job;
                _jobDetail = jobDetail;
            }
            
            public async Task Execute(IJobExecutionContext context)
            {
                using (_logger.BeginScope(new Dictionary<string, object>()
                {
                    {"Job", _jobDetail.Key.Name},
                    {"JobGroup", _jobDetail.Key.Group}
                }))
                {
                    try
                    {
                        await _job.Execute(context);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Unhandled job exception ({e.GetType()}) [{e.Message}]", e);
                    }
                }
            }
        }
    }

    internal interface IScopedJob
    {
        IServiceScope Scope { get; }
    }
}