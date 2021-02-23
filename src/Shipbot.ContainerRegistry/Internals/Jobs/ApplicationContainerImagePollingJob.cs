using System.Threading.Tasks;
using Shipbot.Applications;
using Shipbot.Contracts;
using Shipbot.JobScheduling;

namespace Shipbot.ContainerRegistry.Internals.Jobs
{
    /// <summary>
    ///     Enumerates applications and adds any new image repository detected.  Really effective when
    ///     we implement dynamic application management (for now Applications are loaded on startup from config files)
    /// </summary>
    internal class ApplicationContainerImagePollingJob : BaseJob
    {
        private readonly IApplicationService _applicationService;
        private readonly IRegistryWatcher _registryWatcher;

        public ApplicationContainerImagePollingJob(
            IApplicationService applicationService,
            IRegistryWatcher registryWatcher
            )
        {
            _applicationService = applicationService;
            _registryWatcher = registryWatcher;
        }
        public override async Task Execute()
        {
            var allApplications = _applicationService.GetApplications();

            foreach (var application in allApplications)
            {
                foreach (var applicationImage in application.Images)
                {
                    var repositoryAlreadyTracked = await _registryWatcher.IsWatched(applicationImage.Repository);
                    if (!repositoryAlreadyTracked)
                    {
                        await _registryWatcher.StartWatchingImageRepository(applicationImage.Repository);   
                    }
                }
            }
        }
    }
}