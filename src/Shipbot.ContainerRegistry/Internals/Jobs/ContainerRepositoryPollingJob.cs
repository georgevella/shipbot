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

            using (_log.BeginScope(new Dictionary<string, object>()
            {
                {"Repository", containerRepository}
            }))
            {
                try
                {
                    _log.LogTrace("Fetching tags for {imagerepository}", containerRepository);

                    var client = await _registryClientPool.GetRegistryClientForRepository(containerRepository);
                    var remoteContainerRepositoryTags = await client.GetRepositoryTags(containerRepository);
                    var localContainerRepositoryTags =
                        await _containerImageMetadataService.GetTagsForRepository(containerRepository);

                    // remove tags and container images that we already know about
                    var newOrUpdatedContainerRepositoryTagsQuery = remoteContainerRepositoryTags
                        .Except(localContainerRepositoryTags);

                    // we don't need tags older than a month, so we'll truncate all the junk
                    newOrUpdatedContainerRepositoryTagsQuery = newOrUpdatedContainerRepositoryTagsQuery
                        .OrderByDescending(x => x.CreationDateTime)
                        .Where(x => x.CreationDateTime >= DateTimeOffset.Now.AddDays(-30))
                        .Take(20);

                    // ReSharper disable once PossibleMultipleEnumeration
                    var newOrUpdatedContainerRepositoryTags = newOrUpdatedContainerRepositoryTagsQuery.ToList();
                    if (newOrUpdatedContainerRepositoryTags.Any())
                    {
                        _log.LogInformation($"Adding '{newOrUpdatedContainerRepositoryTags.Count}' new image tags ...");
                        await _containerImageMetadataService.AddOrUpdate(newOrUpdatedContainerRepositoryTags);
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