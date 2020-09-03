using System.Threading.Tasks;
using Quartz;

namespace Shipbot.JobScheduling
{
    public abstract class BaseJobWithData<T> : IJob
    {
        public abstract Task Execute(T data);
        
        Task IJob.Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;
            var data = dataMap.GetJobData<T>();

            return Execute(data);
        }
    }
}