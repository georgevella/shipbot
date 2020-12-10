using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Shipbot.ContainerRegistry.Services;
using Shipbot.JobScheduling;

namespace Shipbot.ContainerRegistry.Watcher
{
    [DisallowConcurrentExecution]
    internal class ContainerRegistryPollingJob : BaseJobWithData<ContainerRepositoryPollingContext>
    {
        private readonly ILogger<ContainerRegistryPollingJob> _log;
        private readonly IRegistryClientPool _registryClientPool;
        private readonly IContainerImageMetadataService _containerImageMetadataService;
        
        public ContainerRegistryPollingJob(
            ILogger<ContainerRegistryPollingJob> log,
            IRegistryClientPool registryClientPool, 
            IContainerImageMetadataService containerImageMetadataService
        )
        {
            _log = log;
            _registryClientPool = registryClientPool;
            _containerImageMetadataService = containerImageMetadataService;
        }

        public override async Task Execute(ContainerRepositoryPollingContext context)
        {
            var containerRepository = context.ContainerRepository;
            // var applicationId = context.ApplicationId;
            // var imageIndex = context.ImageIndex;
            //
            // var application = _applicationService.GetApplication(applicationId);
            // var image = application.Images[imageIndex];

            using (_log.BeginScope(new Dictionary<string, object>()
            {
                {"Repository", containerRepository}
            }))
            {
                // var currentTags = _applicationService.GetCurrentImageTags(application);
                //
                // if (!currentTags.ContainsKey(image))
                // {
                //     _log.LogInformation(
                //         "Current Tag not available, application source watcher may have not yet run or detected the current image tag"
                //         );
                // }
                // else
                // {
                    try
                    {
                        // var currentTag = currentTags[image];

                        _log.LogInformation("Fetching tags for {imagerepository}", containerRepository);
                        
                        var client = await _registryClientPool.GetRegistryClientForRepository(containerRepository);
                        // var currentImage = await client.GetImage(image.Repository, currentTag);
                        var remoteContainerRepositoryTags = await client.GetRepositoryTags(containerRepository);
                        var localContainerRepositoryTags = await _containerImageMetadataService.GetTagsForRepository(containerRepository);

                        var newOrUpdatedContainerRepositoryTags = remoteContainerRepositoryTags
                            .Except(localContainerRepositoryTags).ToList();
                        
                        _log.LogInformation($"Adding '{newOrUpdatedContainerRepositoryTags.Count}' new image tags ...");
                        foreach (var tag in newOrUpdatedContainerRepositoryTags)
                        {
                            await _containerImageMetadataService.AddOrUpdate(tag);
                        }
                        
                        // var latestImage = _newContainerImageService.GetLatestTagMatchingPolicy(remoteContainerRepositoryTags, image.Policy);
                        
                        // var comparer = _newContainerImageService.GetComparer(image.Policy);
                        
                        // if (comparer.Compare(currentImage, latestImage) < 0)
                        // {
                        //     // latest image is newer than current image
                        //     _log.LogInformation(
                        //         "A new image {latestImageTag} is available for image {imagename} on app {application} (replacing {currentTag})",
                        //         latestImage.Tag, image.Repository, application.Name, currentTag);
                        //     await _deploymentService.AddDeployment(application, image, latestImage.Tag);
                        // }
                    }
                    catch (Exception e)
                    {
                        _log.LogWarning(e, "An error occured when fetching the latest image tags from the registry.");
                    }
                // }
            }
        }
    }
}