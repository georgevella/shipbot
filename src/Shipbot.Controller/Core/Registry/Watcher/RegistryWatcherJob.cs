using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Shipbot.Controller.Core.ApplicationSources;
using Shipbot.Controller.Core.Apps;
using Shipbot.Controller.Core.Deployments;

namespace Shipbot.Controller.Core.Registry.Watcher
{
    [DisallowConcurrentExecution]
    class RegistryWatcherJob : IJob
    {
        private readonly ILogger<RegistryWatcherJob> _log;
        private readonly RegistryClientPool _registryClientPool;
        private readonly IApplicationService _applicationService;
        private readonly IDeploymentService _deploymentService;

        public RegistryWatcherJob(
            ILogger<RegistryWatcherJob> log,
            RegistryClientPool registryClientPool, 
            IApplicationService applicationService,
            IDeploymentService deploymentService
        )
        {
            _log = log;
            _registryClientPool = registryClientPool;
            _applicationService = applicationService;
            _deploymentService = deploymentService;
        }
            
        public async Task Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;

            var imageRepository = (string) dataMap["ImageRepository"];
            var applicationId = (string) dataMap["Application"];
            var imageIndex = (int) dataMap["ImageIndex"];

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
                        "Current Tag not available, application source watcher may have not yet run or detected the current image tag");
                }
                else
                {
                    try
                    {
                        var currentTag = currentTags[image];

                        _log.LogInformation("Fetching tags for {imagerepository}", imageRepository);
                        var client = await _registryClientPool.GetRegistryClientForRepository(imageRepository);

                        var tags = await client.GetRepositoryTags(imageRepository);

                        var matchingTags = tags.Where(tagDetails => image.Policy.IsMatch(tagDetails.tag))
                            .ToDictionary(x => x.tag);

                        var latestTag = matchingTags.Values
                            .OrderBy(tuple => tuple.createdAt, Comparer<DateTime>.Default)
                            .Last();

                        if (latestTag.tag == currentTag)
                        {
                            _log.LogInformation("Latest image tag is applied to the deployment specs");
                        }
                        else
                        {
                            _log.LogInformation(
                                "A new image {latestImageTag} is available for image {imagename} on app {application} (replacing {currentTag})",
                                latestTag.tag, image.Repository, application.Name, currentTag);
                            await _deploymentService.AddDeploymentUpdate(application, image, latestTag.tag);
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