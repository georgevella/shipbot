using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shipbot.Applications;
using Shipbot.Controller.Core.Deployments;
using Shipbot.Controller.DTOs;
using Shipbot.Models.Deployments;

namespace Shipbot.Controller.Controllers
{
    [Route("api/deployments/")]
    [ApiController]
    public class DeploymentsController: ControllerBase
    {
        private readonly IApplicationService _applicationService;
        private readonly IDeploymentService _deploymentService;

        public DeploymentsController(
            IApplicationService applicationService,
            IDeploymentService deploymentService
            )
        {
            _applicationService = applicationService;
            _deploymentService = deploymentService;
        }

        [HttpGet("{application}")]
        public async Task<ActionResult<IEnumerable<ApplicationDeploymentDto>>> GetDeployments(string application)
        {
            try
            {
                var appModel = _applicationService.GetApplication(application);
                var deployments = await _deploymentService.GetDeployments(appModel);

                var result = deployments.Select(x => new ApplicationDeploymentDto()
                {
                    Id = x.Id,
                    Repository = x.ImageRepository,
                    CurrentTag = x.CurrentTag,
                    Tag = x.TargetTag,
                    UpdatePath = x.UpdatePath,
                    Status = (DeploymentStatusDto)x.Status
                }).ToList();

                return Ok(result.AsEnumerable());
            }
            catch (Exception e)
            {
                return NotFound();
            }
        }
    }
}