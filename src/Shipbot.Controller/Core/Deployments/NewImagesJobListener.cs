using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using Quartz.Listener;
using Shipbot.Controller.Core.Jobs;
using Shipbot.Controller.Core.Registry.Watcher;

namespace Shipbot.Controller.Core.Deployments
{
    public class NewImagesJobListener : JobListenerSupport
    {
        private readonly IDeploymentService _deploymentService;

        public NewImagesJobListener(IDeploymentService deploymentService)
        {
            _deploymentService = deploymentService;
        }

        public override Task JobWasExecuted(
            IJobExecutionContext context,
            JobExecutionException jobException,
            CancellationToken cancellationToken = new CancellationToken()
        )
        {
            if (context.JobInstance is JobWrapper<RegistryWatcherJob>)
            {
                var repository = context.MergedJobDataMap.GetString("ImageRepository");
                if (context.Result is IEnumerable<ImageTag> newTags)
                {
                    if (newTags.Any())
                    {
                        _deploymentService.AddDeploymentUpdate(repository, newTags);
                    }
                }
            }

            return base.JobWasExecuted(context, jobException, cancellationToken);
        }

        public override string Name { get; } = nameof(NewImagesJobListener);
    }
}