using System.Threading.Tasks;
using Quartz;

namespace Shipbot.JobScheduling
{
    public abstract class BaseJobWithData<T> : IJob
    {
        public abstract Task Execute(T context);
        
        Task IJob.Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;
            var data = dataMap.GetJobData<T>();

            return Execute(data);
        }
    }

    public abstract class BaseJob : IJob
    {
        public abstract Task Execute();

        Task IJob.Execute(IJobExecutionContext context)
        {
            return Execute();
        }
    }
}