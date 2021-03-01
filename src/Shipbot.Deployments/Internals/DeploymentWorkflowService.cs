using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Octokit;
using Shipbot.Applications;
using Shipbot.Applications.Models;
using Shipbot.Common;
using Shipbot.ContainerRegistry.Models;
using Shipbot.ContainerRegistry.Services;
using Shipbot.Deployments.Models;
using Application = Shipbot.Applications.Models.Application;
using Deployment = Shipbot.Deployments.Models.Deployment;

namespace Shipbot.Deployments.Internals
{
    internal class DeploymentWorkflowService : IDeploymentWorkflowService
    {
        private readonly ILogger<DeploymentWorkflowService> _log;
        private readonly IContainerImageMetadataService _containerImageMetadataService;
        private readonly IApplicationService _applicationService;
        private readonly IApplicationImageInstanceService _applicationImageInstanceService;
        private readonly IDeploymentService _deploymentService;
        private readonly IDeploymentQueueService _deploymentQueueService;
        private readonly IGitHubClient _gitHubClient;

        public DeploymentWorkflowService(
            ILogger<DeploymentWorkflowService> log,
            IContainerImageMetadataService containerImageMetadataService,
            IApplicationService applicationService,
            IApplicationImageInstanceService applicationImageInstanceService,
            IDeploymentService deploymentService,
            IDeploymentQueueService deploymentQueueService,
            IGitHubClient gitHubClient
            )
        {
            _log = log;
            _containerImageMetadataService = containerImageMetadataService;
            _applicationService = applicationService;
            _applicationImageInstanceService = applicationImageInstanceService;
            _deploymentService = deploymentService;
            _deploymentQueueService = deploymentQueueService;
            _gitHubClient = gitHubClient;
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
            var allApplicationsTrackingThisRepository = GetAllApplicationsTrackingThisRepository(latestImage.Repository);
            
            return InternalStartImageDeployment(
                allApplicationsTrackingThisRepository,
                latestImage.Repository, 
                new List<ContainerImage>{latestImage}, 
                isContainerRepositoryUpdate
                );
        }

        public Task<IEnumerable<Deployment>> StartImageDeployment(
            string applicationName, 
            ContainerImage containerImage,
            bool isContainerRepositoryUpdate = false)
        {
            var application = _applicationService.GetApplication(applicationName);
            var targetImages = application.Images
                .Where(image => image.Repository.Equals(containerImage.Repository))
                .Select(image => (image, application))
                .ToList();
            
            return InternalStartImageDeployment(
                targetImages,
                containerImage.Repository, 
                new List<ContainerImage>{containerImage}, 
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

            if (items.Count == 0)
            {
                throw new InvalidOperationException(
                    "No container images where supplied for deployment.");
            }
            
            if (!items
                .Select(x => x.Repository.ToLowerInvariant())
                .All(x => x.Equals(containerImageRepository.ToLower()))
            )
            {
                throw new InvalidOperationException(
                    "Not all container images supplied are within the same container repository.");
            }
            
            var allApplicationsTrackingThisRepository = GetAllApplicationsTrackingThisRepository(containerImageRepository);
            
            return InternalStartImageDeployment(
                allApplicationsTrackingThisRepository,
                containerImageRepository, 
                items,
                isContainerRepositoryUpdate
                );
        } 
        
        private async Task<IEnumerable<Deployment>> InternalStartImageDeployment(
            IEnumerable<(ApplicationImage Image, Application Application)> targetApplicationsAndImages, 
            string containerImageRepository,
            IReadOnlyCollection<ContainerImage> newContainerImages,
            bool isContainerRepositoryUpdate = false
        )
        {
            var createdDeployments = new List<Deployment>();

            foreach (var item in targetApplicationsAndImages)
            {
                if (
                    isContainerRepositoryUpdate &&
                    !item.Image.DeploymentSettings.AutomaticallyCreateDeploymentOnImageRepositoryUpdate
                    )
                {
                    // we received a container repository update, but the AutomaticallyCreateDeploymentOnRepositoryUpdate flag
                    // is set to false, thus we need to skip this application.
                    continue;
                }
                
                /*
                 * handle preview release
                 */

                try
                {
                    await HandlePreviewReleaseImages(newContainerImages, item, createdDeployments);
                }
                catch (Exception e)
                {
                    _log.LogError("Failed to handle preview releases", e);
#if DEBUG
                    throw;
#endif
                }

                /*
                 * handle primary release
                 */
                try
                {
                    // fetch current deployed image information
                    var currentTagInStore = await _applicationImageInstanceService.GetCurrentTagForPrimary(item.Application, item.Image);
                    
                    // TODO: we may miss some images if the deployment source services take long to load the current tags
                    if (!currentTagInStore.available)
                        continue;

                    var currentTag = currentTagInStore.tag;
                    var deployment = await HandlePrimeInstanceImages(newContainerImages, isContainerRepositoryUpdate, item, currentTag);
                    if (deployment != null)
                        createdDeployments.Add(deployment);
                }
                catch (Exception e)
                {
                    _log.LogError("Failed to start deployment workflow", e);
#if DEBUG
                    throw;
#endif
                }
            }

            return createdDeployments;
        }

        private async Task HandlePreviewReleaseImages(IReadOnlyCollection<ContainerImage> newContainerImages,
            (ApplicationImage Image, Application Application) item, List<Deployment> createdDeployments)
        {
            var allApplicationImagesWithPreviewReleases = item.Application.Images
                .Where(x => x.DeploymentSettings.PreviewReleases.Enabled)
                .ToList();

            foreach (var appImageWithPreviewRelease in allApplicationImagesWithPreviewReleases)
            {
                var deploymentSettingsPreviewReleases = appImageWithPreviewRelease.DeploymentSettings.PreviewReleases;
                
                // preview releases depend on an open PR, so we need the application source code settings
                if (!appImageWithPreviewRelease.SourceCode.IsAvailable)
                {
                    _log.LogWarning(
                        "Preview deployments are enabled for {ApplicationImage} but now source code settings set", 
                        appImageWithPreviewRelease.Repository
                        );
                    continue;
                }
                
                var previewReleaseContainerImages = newContainerImages
                    .Where(
                        tagDetails => deploymentSettingsPreviewReleases.Policy.IsMatch(tagDetails.Tag)
                    )
                    .Select(
                        image => new
                        {
                            Image = image,
                            TagParameters =
                                image.Tag.ExtractParametersWithRegex(deploymentSettingsPreviewReleases
                                    .TagPatternRegex)
                        }
                    )
                    .Select(
                        x => new
                        {
                            Image = x.Image,
                            TagParameters = x.TagParameters,
                            Branch = x.TagParameters.GetValueOrDefault("branch", string.Empty),
                            BuildNumber = x.TagParameters.GetValueOrDefault("buildNumber", string.Empty),
                            Version = x.TagParameters.GetValueOrDefault("version", string.Empty)
                        }
                    )
                    .GroupBy(x => x.Branch);

                var pullRequests = await _gitHubClient.PullRequest.GetAllForRepository(
                    appImageWithPreviewRelease.SourceCode.Github.Owner,
                    appImageWithPreviewRelease.SourceCode.Github.Repository,
                    new PullRequestRequest()
                    {
                        State = ItemStateFilter.Open
                    });
                
                var pullRequestMap = pullRequests.Select(x => (
                            prObject: x,
                        branch: x.Head.Label, 
                        number: x.Number, 
                        isOpen: x.State.Value == ItemState.Open, 
                        id: x.Id,
                        title: x.Title,
                        creator: x.User.Email ?? x.User.Name
                        )
                    )
                    .ToList();

                foreach (var group in previewReleaseContainerImages)
                {
                    var branch = @group.Key;
                    
                    var latestImage = @group
                        .OrderByDescending(x => x.Image.CreationDateTime)
                        .First();

                    // determine PR number and use that as the instance id
                    var matchingPullrequests = pullRequestMap
                        .Where(x => x.branch.Contains(branch, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (matchingPullrequests.Count > 1)
                    {
                        _log.LogCritical("Multiple pull requests or branches with the same base name ('{Branch}') were detected", branch);
                        continue;
                    }
                    if (matchingPullrequests.Count == 0)
                    {
                        _log.LogDebug("No open pull requests with the base name ('{Branch}') were detected, skipping image deployment", branch);
                        continue;
                    }

                    var relevantPullRequest = matchingPullrequests[0];

                    var instanceId = $"pr-{relevantPullRequest.number}";

                    // check if a deployment for this image is already present
                    var present = await _deploymentService.IsDeploymentPresent(
                        item.Application,
                        item.Image,
                        latestImage.Image.Tag,
                        DeploymentType.PreviewRelease,
                        instanceId);

                    if (present)
                    {
                        _log.LogTrace("Skipping deployment of preview release for branch '{Branch}' because it is already present for tag {Tag}", branch, latestImage.Image.Tag);
                        continue;
                    }

                    // fetch current deployed image information
                    var currentTagInStore =
                        await _applicationImageInstanceService.GetCurrentTag(item.Application, item.Image, instanceId);

                    // TODO: we may miss some images if the deployment source services take long to load the current tags
                    var createDeployment = false;
                    string? targetTag = null;

                    if (!currentTagInStore.available)
                    {
                        // we don't have a FIRST release for this Preview image
                        createDeployment = item.Image.DeploymentSettings
                            .AutomaticallyCreateDeploymentOnImageRepositoryUpdate;
                        targetTag = latestImage.Image.Tag;
                    }
                    else
                    {
                        // we already have a release for this Preview image, let's see if image is more recent
                        var currentImage =
                            await _containerImageMetadataService.TryGetContainerImageByTag(item.Image.Repository,
                                currentTagInStore.tag);

                        if (currentImage.success)
                        {
                            // start comparison with latest container image.
                            var comparer = GetContainerImageComparer(item.Image.Policy);
                            createDeployment = comparer.Compare(currentImage.image, latestImage.Image) < 0;
                            targetTag = createDeployment ? latestImage.Image.Tag : null;
                        }
                        else
                        {
                            // createDeployment = latestImage.success;
                            // targetTag = latestImage.success ? latestImage.image.Tag : null;
                            // TODO: this may need some better handling
                            createDeployment = true;
                            targetTag = latestImage.Image.Tag;
                        }
                    }

                    if (!createDeployment || targetTag == null)
                    {
                        _log.LogTrace("Skipping deployment of preview release for branch '{Branch}' with tag '{Tag}' because conditions where not met", branch, latestImage.Image.Tag);
                        continue;
                    };
                    
                    var deployment = await _deploymentService.AddDeployment(
                        item.Application,
                        item.Image,
                        latestImage.Image.Tag,
                        DeploymentType.PreviewRelease,
                        instanceId,
                        DeploymentParametersBuilder.Build(
                            DeploymentType.PreviewRelease,
                            instanceId,
                                latestImage.TagParameters, 
                                relevantPullRequest.prObject,
                                item.Image)
                            .ToDictionary()
                        );

                    // register deployment
                    createdDeployments.Add(deployment);

                    _log.LogTrace("Adding preview release deployment for branch '{Branch}' with tag '{Tag}' to deployment queue", branch, latestImage.Image.Tag);
                    // push deployment onto the queue since we have AutoDeploy set
                    await _deploymentQueueService.EnqueueDeployment(deployment);
                }
            }
        }

        private async Task<Deployment?> HandlePrimeInstanceImages(
            IEnumerable<ContainerImage> newContainerImages,
            bool isContainerRepositoryUpdate, 
            (ApplicationImage Image, Application Application) item, 
            string currentTag
            )
        {
            string? targetTag = null;
            var createDeployment = false;

            if (isContainerRepositoryUpdate)
            {
                /*
                 * we got a container image update from one of the registry polling services.
                 */
                
                // in here we try to determine if the new image received from one of the pollers
                // is newer than the one deployed currently on the service

                var latestImage = TryGetLatestTagMatchingPolicy(newContainerImages, item.Image.Policy);
                var currentImage =
                    await _containerImageMetadataService.TryGetContainerImageByTag(item.Image.Repository, currentTag);

                if (currentImage.success)
                {
                    // start comparison with latest container image.
                    var comparer = GetContainerImageComparer(item.Image.Policy);
                    createDeployment = comparer.Compare(currentImage.image, latestImage.image) < 0;
                    targetTag = createDeployment ? latestImage.image.Tag : null;
                }
                else
                {
                    createDeployment = latestImage.success;
                    targetTag = latestImage.success ? latestImage.image.Tag : null;
                }
            }
            else
            {
                // we probably received a manual deployment request
                var latestImage = TryGetLatestTagMatchingPolicy(newContainerImages, item.Image.Policy);

                createDeployment = latestImage.success;
                targetTag = latestImage.success ? latestImage.image.Tag : null;
            }

            if (createDeployment && targetTag != null)
            {
                // latest image is newer than current image
                _log.LogInformation(
                    "A new image {latestImageTag} is available for image {imagename} on app {application} (replacing {currentTag})",
                    targetTag, item.Image.Repository, item.Application.Name, currentTag);
                var deployment = await _deploymentService.AddDeployment(item.Application, item.Image, targetTag);

                // check if application image is setup for automatic submission of the deployment to the queue.
                if (item.Image.DeploymentSettings.AutomaticallySubmitDeploymentToQueue)
                {
                    _log.LogDebug("Adding deployment to deployment queue.");
                    // push deployment onto the queue since we have AutoDeploy set
                    await _deploymentQueueService.EnqueueDeployment(deployment);
                }

                return deployment;
            }

            return null;
        }

        private IEnumerable<(ApplicationImage Image, Application Application)> GetAllApplicationsTrackingThisRepository(string containerImageRepository)
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
                )
                .Select( x => ( x.Image, x.Application ) )
                .ToList();
            return allApplicationsTrackingThisRepository;
        }

        internal ContainerImage GetLatestTagMatchingPolicy(
            IEnumerable<ContainerImage> images,
            ImageUpdatePolicy imagePolicy
        )
        {
            var matchingTags = images
                .Where(
                    tagDetails => imagePolicy.IsMatch(tagDetails.Tag)
                ).ToList();

            var latestImage = matchingTags
                .OrderBy(i => i.CreationDateTime, Comparer<DateTimeOffset>.Default)
                .Last();

            return latestImage;
        }

        internal (bool success, ContainerImage image) TryGetLatestTagMatchingPolicy(
            IEnumerable<ContainerImage> images,
            ImageUpdatePolicy imagePolicy
        )
        {
            try
            {
                var image = GetLatestTagMatchingPolicy(images, imagePolicy);
                return (true, image);
            }
            catch
            {
                return (false, ContainerImage.Empty);
            }
        }

        internal IComparer<ContainerImage> GetContainerImageComparer(ImageUpdatePolicy updatePolicy)
        {
            return updatePolicy switch
            {
                GlobImageUpdatePolicy globImageUpdatePolicy =>
                    Comparer<ContainerImage>.Create(
                        (x, y) => x.Equals(y) ? 0 : x.CreationDateTime.CompareTo(y.CreationDateTime)),
                RegexImageUpdatePolicy regexImageUpdatePolicy =>
                    Comparer<ContainerImage>.Create(
                        (x, y) => x.Equals(y) ? 0 : x.CreationDateTime.CompareTo(y.CreationDateTime)),
                _ => throw new ArgumentOutOfRangeException(nameof(updatePolicy))
            };
        }

    }
}