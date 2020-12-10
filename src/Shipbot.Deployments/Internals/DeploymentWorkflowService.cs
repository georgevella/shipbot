using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shipbot.Applications;
using Shipbot.ContainerRegistry;
using Shipbot.ContainerRegistry.Models;
using Shipbot.ContainerRegistry.Services;
using Shipbot.Deployments.Models;

namespace Shipbot.Deployments.Internals
{
    internal class DeploymentWorkflowService : IDeploymentWorkflowService
    {
        private readonly ILogger<DeploymentWorkflowService> _log;
        private readonly IContainerImageMetadataService _containerImageMetadataService;
        private readonly INewContainerImageService _newContainerImageService;
        private readonly IApplicationService _applicationService;
        private readonly IDeploymentService _deploymentService;
        private readonly IDeploymentQueueService _deploymentQueueService;

        public DeploymentWorkflowService(
            ILogger<DeploymentWorkflowService> log,
            IContainerImageMetadataService containerImageMetadataService,
            INewContainerImageService newContainerImageService,
            IApplicationService applicationService,
            IDeploymentService deploymentService,
            IDeploymentQueueService deploymentQueueService
            )
        {
            _log = log;
            _containerImageMetadataService = containerImageMetadataService;
            _newContainerImageService = newContainerImageService;
            _applicationService = applicationService;
            _deploymentService = deploymentService;
            _deploymentQueueService = deploymentQueueService;
        }

        /// <summary>
        ///     Handles the logic required to deploy a new container image to one or more applications.
        ///     <remarks>
        ///         A new container image can be either detected by the repository polling jobs, OR through manual
        ///         submission from the Administrative REST API. 
        ///     </remarks> 
        /// </summary>
        /// <param name="latestImage"></param>
        /// <param name="isContainerRepositoryUpdate"></param>
        /// <returns>List of deployments that were created.</returns>
        public Task<IEnumerable<Deployment>> StartImageDeployment(
            ContainerImage latestImage,
            bool isContainerRepositoryUpdate = false
        )
        {
            return InternalStartImageDeployment(
                latestImage.Repository, 
                new List<ContainerImage>{latestImage}, 
                isContainerRepositoryUpdate
                );
        }
        
        public Task<IEnumerable<Deployment>> StartImageDeployment(
            string containerImageRepository,
            IEnumerable<ContainerImage> newContainerImages,
            bool isContainerRepositoryUpdate = false
        )
        {
            var items = newContainerImages.ToList();
            // sanity check
            if (!items
                .Select(x => x.Repository.ToLowerInvariant())
                .All(x => x.Equals(containerImageRepository.ToLower()))
            )
            {
                throw new InvalidOperationException(
                    "Not all container images supplied are within the same container repository.");
            }
            
            return InternalStartImageDeployment(
                containerImageRepository, 
                items,
                isContainerRepositoryUpdate
                );

        }      
        
        private async Task<IEnumerable<Deployment>> InternalStartImageDeployment(
            string containerImageRepository,
            IReadOnlyCollection<ContainerImage> newContainerImages,
            bool isContainerRepositoryUpdate = false
        )
        {
            var applications = _applicationService.GetApplications();
            var allApplicationsTrackingThisRepository = applications
                .SelectMany(
                    x => x.Images,
                    (app, img) =>
                        new
                        {
                            Image = img,
                            Application = app
                        }
                )
                .Where(x =>
                    x.Image.Repository.Equals(containerImageRepository)
                );

            var createdDeployments = new List<Deployment>();

            foreach (var item in allApplicationsTrackingThisRepository)
            {
                if (
                    isContainerRepositoryUpdate &&
                    !item.Image.DeploymentSettings.AutomaticallyCreateDeploymentOnRepositoryUpdate
                    )
                {
                    // we received a container repository update, but the AutomaticallyCreateDeploymentOnRepositoryUpdate flag
                    // is set to false, thus we need to skip this application.
                    continue;
                }
                
                try
                {
                    // fetch current deployed image information
                    var currentTags = _applicationService.GetCurrentImageTags(item.Application);
                    var currentTag = currentTags[item.Image];
                    string? targetTag = null;
                    var createDeployment = false;
                    
                    if (isContainerRepositoryUpdate)
                    {
                        var currentImage = await _containerImageMetadataService.GetContainerImageByTag(item.Image.Repository, currentTag);
            
                        // start comparison with latest container image.
                        var latestImage =  _newContainerImageService
                            .GetLatestTagMatchingPolicy(newContainerImages, item.Image.Policy);
                    
                        var comparer = _newContainerImageService.GetComparer(item.Image.Policy);
                        createDeployment = comparer.Compare(currentImage, latestImage) < 0;
                        targetTag = latestImage.Tag;
                    }
                    else
                    {
                        var singleContainerImage = newContainerImages.Single();
                        if (item.Image.Policy.IsMatch(singleContainerImage.Tag))
                        {
                            createDeployment = true;
                            targetTag = singleContainerImage.Tag;
                        }
                    }
                    
                    if (createDeployment && targetTag != null)
                    {
                        // latest image is newer than current image
                        _log.LogInformation(
                            "A new image {latestImageTag} is available for image {imagename} on app {application} (replacing {currentTag})",
                            targetTag, item.Image.Repository, item.Application.Name, currentTag);
                        var deployment =  await _deploymentService.AddDeployment(item.Application, item.Image, targetTag);
                        
                        // check if application image is setup for automatic submission of the deployment to the queue.
                        if (item.Image.DeploymentSettings.AutomaticallySubmitDeploymentToQueue)
                        {
                            _log.LogDebug("Adding deployment to deployment queue.");
                            // push deployment onto the queue since we have AutoDeploy set
                            await _deploymentQueueService.EnqueueDeployment(deployment);
                        }
                        
                        createdDeployments.Add(deployment);
                    }
                }
                catch (Exception e)
                {
                    _log.LogError("Failed to start deployment workflow", e);
                }
            }

            return createdDeployments;
        }

    }
}