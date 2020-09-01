using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shipbot.Applications;
using Shipbot.ContainerRegistry;
using Shipbot.ContainerRegistry.Models;
using Shipbot.ContainerRegistry.Services;
using Shipbot.Deployments;

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

        public DiagnosticsController(
            IApplicationService applicationService,
            IDeploymentService deploymentService,
            IDeploymentNotificationService deploymentNotificationService,
            INewContainerImageService newContainerImageService
            )
        {
            _applicationService = applicationService;
            _deploymentService = deploymentService;
            _deploymentNotificationService = deploymentNotificationService;
            _newContainerImageService = newContainerImageService;
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
    }
}