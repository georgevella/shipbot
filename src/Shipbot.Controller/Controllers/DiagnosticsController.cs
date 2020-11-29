using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shipbot.Applications;
using Shipbot.ContainerRegistry;
using Shipbot.ContainerRegistry.Models;
using Shipbot.ContainerRegistry.Services;
using Shipbot.Controller.DTOs;
using Shipbot.Deployments;
using Shipbot.SlackIntegration;

namespace Shipbot.Controller.Controllers
{
    [Route("api/diag")]
    [ApiController]
    public class DiagnosticsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;
        private readonly IDeploymentService _deploymentService;
        private readonly IDeploymentNotificationService _deploymentNotificationService;
        private readonly INewContainerImageService _newContainerImageService;
        private readonly ISlackClient _slackClient;
        private readonly ILocalContainerMetadataService _localContainerMetadataService;

        public DiagnosticsController(
            IApplicationService applicationService,
            IDeploymentService deploymentService,
            IDeploymentNotificationService deploymentNotificationService,
            INewContainerImageService newContainerImageService,
            ISlackClient slackClient,
            ILocalContainerMetadataService localContainerMetadataService
        )
        {
            _applicationService = applicationService;
            _deploymentService = deploymentService;
            _deploymentNotificationService = deploymentNotificationService;
            _newContainerImageService = newContainerImageService;
            _slackClient = slackClient;
            _localContainerMetadataService = localContainerMetadataService;
        }
        
        [HttpPost("deployment-notifications")]
        public async Task<ActionResult> ResendDeploymentNotification(
            [FromForm(Name = "deploymentId")] Guid deploymentId
            )
        {
            var deployment = await _deploymentService.GetDeployment(deploymentId);
            await _deploymentNotificationService.CreateNotification(deployment);

            return Ok();
        }
        
        [HttpPost("container-registry-new-tag")]
        public async Task<ActionResult> SubmitTagsAsContainerRegistryPoller(
            [FromForm(Name = "application")] string applicationId,
            [FromForm(Name = "repository")] string repository,
            [FromForm(Name = "tag")] string tag
        )
        {
            var application = _applicationService.GetApplication(applicationId);
            var image = application.Images.First(x => x.Repository == repository);

            var result = _newContainerImageService.GetLatestTagMatchingPolicy(new[]
                {
                    new ContainerImage(repository, tag, tag.GetHashCode().ToString()), 
                },
                image.Policy
            );

            return Ok(result);
        }
        
        [HttpGet("container-image-local-store")]
        public async Task<ActionResult> GetContainerImageIntoLocalStore([FromQuery] string repository)
        {
            var containerImages = await _localContainerMetadataService.GetTagsForRepository(repository);

            return Ok(containerImages
                .Select(x => new ContainerImageDto()
                {
                    Hash = x.Hash,
                    Repository = x.Repository,
                    Tag = x.Tag,
                    CreationDateTime = x.CreationDateTime
                })
                .ToList()
            );
        }

        [HttpPost("container-image-local-store")]
        public async Task<ActionResult> AddContainerImageIntoLocalStore(ContainerImageDto dto)
        {
            var containerImage = new ContainerImage(dto.Repository, dto.Tag, dto.Hash, dto.CreationDateTime);

            await _localContainerMetadataService.AddOrUpdate(containerImage);
            
            var containerImages = await _localContainerMetadataService
                .GetTagsForRepository(containerImage.Repository);

            return Ok(
                containerImages
                    .Select(x => new ContainerImageDto()
                    {
                        Hash = x.Hash,
                        Repository = x.Repository,
                        Tag = x.Tag,
                        CreationDateTime = x.CreationDateTime
                    })
                    .ToList()
            );
        }

        [HttpGet("slack/user-groups")]
        public async Task<ActionResult> GetSlackUserGroups()
        {
            var result = await _slackClient.GetAllUserGroupNames();
            return Ok(result.Select(x => x.name));
        }
    }
}