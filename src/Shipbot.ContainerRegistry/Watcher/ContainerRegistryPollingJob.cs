using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Shipbot.Applications;
using Shipbot.ContainerRegistry.Services;
using Shipbot.Deployments;
using Shipbot.JobScheduling;

namespace Shipbot.ContainerRegistry.Watcher
{
    [DisallowConcurrentExecution]
    internal class ContainerRegistryPollingJob : BaseJobWithData<ContainerRegistryPollingData>
    {
        private readonly ILogger<ContainerRegistryPollingJob> _log;
        private readonly IRegistryClientPool _registryClientPool;
        private readonly IApplicationService _applicationService;
        private readonly IDeploymentService _deploymentService;
        private readonly INewContainerImageService _newContainerImageService;

        public ContainerRegistryPollingJob(
            ILogger<ContainerRegistryPollingJob> log,
            IRegistryClientPool registryClientPool, 
            IApplicationService applicationService,
            IDeploymentService deploymentService,
            INewContainerImageService newContainerImageService
        )
        {
            _log = log;
            _registryClientPool = registryClientPool;
            _applicationService = applicationService;
            _deploymentService = deploymentService;
            _newContainerImageService = newContainerImageService;
        }

        public override async Task Execute(ContainerRegistryPollingData data)
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
                        var currentImage = await client.GetImage(image.Repository, currentTag);
                        var images = await client.GetRepositoryTags(imageRepository);
                        
                        var latestImage = _newContainerImageService.GetLatestTagMatchingPolicy(images, image.Policy);
                        
                        var comparer = _newContainerImageService.GetComparer(image.Policy);
                        
                        if (comparer.Compare(currentImage, latestImage) < 0)
                        {
                            // latest image is newer than current image
                            _log.LogInformation(
                                "A new image {latestImageTag} is available for image {imagename} on app {application} (replacing {currentTag})",
                                latestImage.Tag, image.Repository, application.Name, currentTag);
                            await _deploymentService.AddDeployment(application, image, latestImage.Tag);
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