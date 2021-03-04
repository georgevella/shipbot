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
            if (newContainerImages.Any())
            {
                LastContainerImageCheck[context.ContainerImageRepository] = newContainerImages.First().CreationDateTime;
                
                await _deploymentWorkflowService.StartImageDeployment(
                    context.ContainerImageRepository,
                    newContainerImages,
                    true);
            }
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