using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using k8s;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shipbot.Applications;
using Shipbot.Applications.Models;
using Shipbot.ContainerRegistry;
using Shipbot.ContainerRegistry.Models;
using Shipbot.ContainerRegistry.Services;
using Shipbot.Controller.Core.Configuration.Apps;
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
        // private readonly IKubernetes _kubernetes;

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
        public async Task<ActionResult> GetContainerImageIntoLocalStore([FromQuery] string repository, [FromQuery] string pattern)
        {
            if (string.IsNullOrEmpty(repository))
                return BadRequest();
            
            var containerImages = await _containerImageMetadataService.GetTagsForRepository(repository);

            if (string.IsNullOrWhiteSpace(pattern))
            {
                return Ok(new
                {
                    Items = containerImages.Select(x => (ContainerImageDto) x).ToList()
                });
            }

            var parts = pattern.Split(':');

            if (parts.Length == 0)
                throw new InvalidOperationException("wtf?");

            var actualPattern = pattern;

            if (!Enum.TryParse<UpdatePolicy>(parts[0], ignoreCase: true, out var policyType))
            {
                // first part is not a policy type, handle later

            }
            else
            {
                // since above we split the pattern with ':', let's assume that the type is part[0],
                // while the pattern is part[1..end].  If there are more than two parts, then the pattern contained
                // ':', thus we need to rebuild the pattern as is by joining all the parts together.
                actualPattern = string.Join(":", parts.Skip(1));
            }

            var policy = policyType switch
            {
                UpdatePolicy.Glob => (ImageUpdatePolicy) new GlobImageUpdatePolicy(actualPattern),
                UpdatePolicy.Regex => new RegexImageUpdatePolicy(actualPattern),
                UpdatePolicy.Semver => new SemverImageUpdatePolicy(actualPattern),
                _ => null
            };

            if (policy == null)
            {
                return BadRequest();
            }

            return Ok(new
            {
                policy = policy,
                Items = containerImages
                    .Where(x => policy.IsMatch(x.Tag))
                    .OrderByDescending(x => x.CreationDateTime)
                    .Select(x => (ContainerImageDto) x)
                    .ToList()
            });



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