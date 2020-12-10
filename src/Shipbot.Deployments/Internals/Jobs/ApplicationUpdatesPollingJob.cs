using System;
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
            var allApplications = _applicationService.GetApplications();

            foreach (var application in allApplications)
            {
                foreach (var appImage in application.Images)
                {
                    if (appImage.DeploymentSettings.AutomaticallyCreateDeploymentOnRepositoryUpdate)
                    {
                        // application and image are set up to automatically track the image repository for updates.
                        // let's check if we already have a job tracking this repository, and if not, set one up.
                        var jobExists = await _scheduler.CheckJobExists<ContainerImageRepositoryPollingJob>(
                            appImage.Repository,
                            JobGroup
                            );
                        
                        if (!jobExists)
                        {
                            await _scheduler
                                .StartRecurringJob<ContainerImageRepositoryPollingJob,
                                    ContainerImageRepositoryPollingJobContext>(
                                    appImage.Repository,
                                    JobGroup,
                                    new ContainerImageRepositoryPollingJobContext(appImage.Repository),
                                    TimeSpan.FromSeconds(2)
                                );
                        }
                    }
                }
            }
        }
    }
}