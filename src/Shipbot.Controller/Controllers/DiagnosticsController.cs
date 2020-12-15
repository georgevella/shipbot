using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IDeploymentService _deploymentService;
        private readonly IDeploymentNotificationService _deploymentNotificationService;
        private readonly ISlackClient _slackClient;
        private readonly IContainerImageMetadataService _containerImageMetadataService;

        public DiagnosticsController(
            IDeploymentService deploymentService,
            IDeploymentNotificationService deploymentNotificationService,
            ISlackClient slackClient,
            IContainerImageMetadataService containerImageMetadataService
        )
        {
            _deploymentService = deploymentService;
            _deploymentNotificationService = deploymentNotificationService;
            _slackClient = slackClient;
            _containerImageMetadataService = containerImageMetadataService;
        }
        
        [HttpPost("deployment-notifications")]
        [Authorize]
        public async Task<ActionResult> ResendDeploymentNotification(
            [FromForm(Name = "deploymentId")] Guid deploymentId
            )
        {
            var deployment = await _deploymentService.GetDeployment(deploymentId);
            await _deploymentNotificationService.CreateNotification(deployment);

            return Ok();
        }

        [HttpGet("container-image-local-store")]
        [Authorize]
        public async Task<ActionResult> GetContainerImageIntoLocalStore([FromQuery] string repository)
        {
            var containerImages = await _containerImageMetadataService.GetTagsForRepository(repository);

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
        [Authorize]
        public async Task<ActionResult> AddContainerImageIntoLocalStore(ContainerImageDto dto)
        {
            var containerImage = new ContainerImage(dto.Repository, dto.Tag, dto.Hash, dto.CreationDateTime);

            await _containerImageMetadataService.AddOrUpdate(containerImage);
            
            var containerImages = await _containerImageMetadataService
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
        [Authorize]
        public async Task<ActionResult> GetSlackUserGroups()
        {
            var result = await _slackClient.GetAllUserGroupNames();
            return Ok(result.Select(x => x.name));
        }
    }
}