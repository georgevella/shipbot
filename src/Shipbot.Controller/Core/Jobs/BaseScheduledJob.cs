using System.Threading.Tasks;
using Quartz;

namespace Shipbot.Controller.Core.Jobs
{
    public abstract class BaseScheduledJob<TContext> : IJob
    {
        public async Task Execute(IJobExecutionContext jobExecutionContext)
        {
            var data = jobExecutionContext.MergedJobDataMap;
            var context = (TContext) data["Context"];

            await Execute(context);
        }

        protected abstract Task Execute(TContext context);
    }
}