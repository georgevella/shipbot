using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Schema;
using Microsoft.Extensions.Logging;
using Quartz;
using Shipbot.Applications;
using Shipbot.ContainerRegistry;
using Shipbot.ContainerRegistry.Models;
using Shipbot.ContainerRegistry.Services;
using Shipbot.JobScheduling;
using Shipbot.Models;

namespace Shipbot.Deployments.Internals.Jobs
{
    /// <summary>
    ///     Polls the container image repository metadata for updates to a repository.
    /// </summary>
    [DisallowConcurrentExecution]
    internal class ContainerImageRepositoryPollingJob : BaseJobWithData<ContainerImageRepositoryPollingJobContext>
    {
        private static readonly ConcurrentDictionary<string, DateTimeOffset> LastContainerImageCheck =
            new ConcurrentDictionary<string, DateTimeOffset>();
        
        private readonly ILogger<ContainerImageRepositoryPollingJob> _log;
        private readonly IDeploymentWorkflowService _deploymentWorkflowService;
        private readonly IContainerImageMetadataService _containerImageMetadataService;

        public ContainerImageRepositoryPollingJob(
            ILogger<ContainerImageRepositoryPollingJob> log,
            IContainerImageMetadataService containerImageMetadataService,
            IDeploymentWorkflowService deploymentWorkflowService
        )
        {
            _log = log;
            _deploymentWorkflowService = deploymentWorkflowService;
            _containerImageMetadataService = containerImageMetadataService;
        }
        
        public override async Task Execute(ContainerImageRepositoryPollingJobContext context)
        {
            var allContainerImages = (await _containerImageMetadataService
                .GetTagsForRepository(context.ContainerImageRepository))
                .ToList();


            IEnumerable<ContainerImage> filter = allContainerImages.OrderByDescending(x => x.CreationDateTime);
            
            if (LastContainerImageCheck.TryGetValue(context.ContainerImageRepository, out var lastCheckTime))
            {
                filter = filter.Where( x=>x.CreationDateTime > lastCheckTime);
            }

            var newContainerImages = filter.ToList();

            await _deploymentWorkflowService.StartImageDeployment(
                context.ContainerImageRepository, 
                newContainerImages,
                true);
            
            LastContainerImageCheck[context.ContainerImageRepository] = DateTimeOffset.Now;

            // var latestImage = _newContainerImageService.GetLatestTagMatchingPolicy(containerImages, applicationImage.Policy);
            //
            // // fetch current deployed image information
            // var currentTags = _applicationService.GetCurrentImageTags(application);
            // var currentTag = currentTags[applicationImage];
            //
            // var currentImage = await _containerImageMetadataService.GetContainerImageByTag(applicationImage.Repository, currentTag);
            //
            // // start comparison
            // var comparer = _newContainerImageService.GetComparer(applicationImage.Policy);
            //             
            // if (comparer.Compare(currentImage, latestImage) < 0)
            // {
            //     // latest image is newer than current image
            //     _log.LogInformation(
            //         "A new image {latestImageTag} is available for image {imagename} on app {application} (replacing {currentTag})",
            //         latestImage.Tag, applicationImage.Repository, application.Name, currentTag);
            //     await _deploymentService.AddDeployment(application, applicationImage, latestImage.Tag);
            // }
        }
    }

    internal class ContainerImageRepositoryPollingJobContext
    {
        public ContainerImageRepositoryPollingJobContext(string containerImageRepository)
        {
            ContainerImageRepository = containerImageRepository;
        }

        public string ContainerImageRepository { get; } 
    }
}