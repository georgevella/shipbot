using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Shipbot.Applications;
using Shipbot.Controller.Core.Registry.Internals;
using Shipbot.Controller.Core.Registry.Services;
using Shipbot.Deployments;
using Shipbot.JobScheduling;

namespace Shipbot.Controller.Core.Registry.Watcher
{
    [DisallowConcurrentExecution]
    internal class ContainerRegistryPollingJob : BaseJobWithData<ContainerRegistryPollingData>
    {
        private readonly ILogger<ContainerRegistryPollingJob> _log;
        private readonly IRegistryClientPool _registryClientPool;
        private readonly IApplicationService _applicationService;
        private readonly IDeploymentService _deploymentService;
        private readonly INewImageTagDetector _newImageTagDetector;

        public ContainerRegistryPollingJob(
            ILogger<ContainerRegistryPollingJob> log,
            IRegistryClientPool registryClientPool, 
            IApplicationService applicationService,
            IDeploymentService deploymentService,
            INewImageTagDetector newImageTagDetector
        )
        {
            _log = log;
            _registryClientPool = registryClientPool;
            _applicationService = applicationService;
            _deploymentService = deploymentService;
            _newImageTagDetector = newImageTagDetector;
        }
        
        protected override async Task Execute(ContainerRegistryPollingData data)
        {
            var imageRepository = data.ImageRepository;
            var applicationId = data.ApplicationId;
            var imageIndex = data.ImageIndex;

            var application = _applicationService.GetApplication(applicationId);
            var image = application.Images[imageIndex];

            using (_log.BeginScope(new Dictionary<string, object>()
            {
                {"Application", application.Name},
                {"Repository", image.Repository}
            }))
            {
                var currentTags = _applicationService.GetCurrentImageTags(application);

                if (!currentTags.ContainsKey(image))
                {
                    _log.LogInformation(
                        "Current Tag not available, application source watcher may have not yet run or detected the current image tag"
                        );
                }
                else
                {
                    try
                    {
                        var currentTag = currentTags[image];

                        _log.LogInformation("Fetching tags for {imagerepository}", imageRepository);
                        var client = await _registryClientPool.GetRegistryClientForRepository(imageRepository);

                        var tags = await client.GetRepositoryTags(imageRepository);

                        var (newImageTagAvailable, tag) = _newImageTagDetector.GetLatestTag(tags, currentTag, image.Policy);
                        if (newImageTagAvailable)
                        {
                            _log.LogInformation(
                                "A new image {latestImageTag} is available for image {imagename} on app {application} (replacing {currentTag})",
                                tag, image.Repository, application.Name, currentTag);
                            await _deploymentService.AddDeployment(application, image, tag);
                        }
                    }
                    catch (Exception e)
                    {
                        _log.LogWarning(e, "An error occured when fetching the latest image tags from the registry.");
                    }
                }
            }
        }
    }
}