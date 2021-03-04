using System;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using Shipbot.Applications;
using Shipbot.Contracts;
using Shipbot.JobScheduling;

namespace Shipbot.Deployments.Internals.Jobs
{
    /// <summary>
    ///  Polls for applications and sets up scheduled polling jobs to track repositories.
    /// </summary>
    [DisallowConcurrentExecution]
    internal class ApplicationUpdatesPollingJob : BaseJob
    {
        private readonly IApplicationService _applicationService;
        private readonly IScheduler _scheduler;

        private const string JobGroup = "CIRPollingJobGroup";

        public ApplicationUpdatesPollingJob(
            IApplicationService applicationService,
            IScheduler scheduler
        )
        {
            _applicationService = applicationService;
            _scheduler = scheduler;
        }
        
        public override async Task Execute()
        {
            var polledImageRepositoriesAssociatedWithApplications = _applicationService.GetApplications()
                .SelectMany(x => x.Images)
                .Where( x => x.DeploymentSettings.AutomaticallyCreateDeploymentOnImageRepositoryUpdate )
                .Select(x => x.Repository)
                .Distinct()
                .ToList();

            foreach (var repository in polledImageRepositoriesAssociatedWithApplications)
            {
                // application and image are set up to automatically track the image repository for updates.
                // let's check if we already have a job tracking this repository, and if not, set one up.
                var jobExists = await _scheduler.CheckJobExists<ContainerImageRepositoryPollingJob>(
                    repository,
                    JobGroup
                );

                if (jobExists)
                    continue;
                
                await _scheduler
                    .StartRecurringJob<ContainerImageRepositoryPollingJob,
                        ContainerImageRepositoryPollingJobContext>(
                        repository,
                        JobGroup,
                        new ContainerImageRepositoryPollingJobContext(repository),
                        TimeSpan.FromSeconds(10)
                    );
            }
        }
    }
}