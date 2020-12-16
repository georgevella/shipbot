using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shipbot.Applications;
using Shipbot.Applications.Models;
using Shipbot.ContainerRegistry.Models;
using Shipbot.Controller.DTOs;
using Shipbot.Deployments;
using Shipbot.Deployments.Models;
using Shipbot.Models;

namespace Shipbot.Controller.Controllers
{
    [Route("api/deployments/")]
    [ApiController]
    public class DeploymentsController: ControllerBase
    {
        private readonly IDeploymentWorkflowService _deploymentWorkflowService;
        private readonly IApplicationService _applicationService;
        private readonly IDeploymentService _deploymentService;

        public DeploymentsController(
            IDeploymentWorkflowService deploymentWorkflowService,
            IApplicationService applicationService,
            IDeploymentService deploymentService
            )
        {
            _deploymentWorkflowService = deploymentWorkflowService;
            _applicationService = applicationService;
            _deploymentService = deploymentService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ApplicationDeploymentDto>>> GetDeployments([FromQuery(Name = "application")] string applicationName, [FromQuery(Name = "status")] DeploymentStatus? status)
        {
            try
            {
                Application application = null;
                if (!string.IsNullOrEmpty(applicationName))
                    application = _applicationService.GetApplication(applicationName);
                
                var deployments = await _deploymentService.GetDeployments(application, status);

                var result = deployments.Select(ConvertFromModel).ToList();

                return Ok(result.AsEnumerable());
            }
            catch (Exception e)
            {
                return NotFound();
            }
        }

        private ApplicationDeploymentDto ConvertFromModel(Deployment x)
        {
            return new ApplicationDeploymentDto()
            {
                Id = x.Id,
                Repository = x.ImageRepository,
                CurrentTag = x.CurrentTag,
                Tag = x.TargetTag,
                UpdatePath = x.UpdatePath,
                Status = (DeploymentStatusDto) x.Status,
                Application = x.ApplicationId
            };
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Post([FromBody] NewDeploymentDto newDeploymentDto)
        {
            var containerImage = new ContainerImage(newDeploymentDto.Repository,
                newDeploymentDto.Tag);
            var createdDeployments = (await _deploymentWorkflowService.StartImageDeployment(containerImage, false)).ToList();

            if (createdDeployments.Any())
            {
                return StatusCode(StatusCodes.Status201Created, createdDeployments.Select(ConvertFromModel).ToList());
            }

            return Ok();
        }
    }
}