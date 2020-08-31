using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shipbot.Applications;
using Shipbot.Controller.Core.Registry.Services;
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
        private readonly INewImageTagDetector _newImageTagDetector;

        public DiagnosticsController(
            IApplicationService applicationService,
            IDeploymentService deploymentService,
            IDeploymentNotificationService deploymentNotificationService,
            INewImageTagDetector newImageTagDetector
            )
        {
            _applicationService = applicationService;
            _deploymentService = deploymentService;
            _deploymentNotificationService = deploymentNotificationService;
            _newImageTagDetector = newImageTagDetector;
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
            var currentTags = _applicationService.GetCurrentImageTags(application);
            var currentTag = currentTags[image];
            var result = _newImageTagDetector.GetLatestTag(new[]
                {
                    (tag, DateTime.Now)
                }, currentTag,
                image.Policy
            );

            return Ok(result);
        }
    }
}