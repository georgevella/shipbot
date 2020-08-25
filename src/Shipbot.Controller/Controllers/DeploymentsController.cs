using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shipbot.Applications;
using Shipbot.Controller.DTOs;
using Shipbot.Deployments;
using Shipbot.Deployments.Models;

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

        [HttpGet("{applicationName}")]
        public async Task<ActionResult<IEnumerable<ApplicationDeploymentDto>>> GetDeployments(string applicationName)
        {
            try
            {
                var application = _applicationService.GetApplication(applicationName);
                var deployments = await _deploymentService.GetDeployments(application);

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
                Status = (DeploymentStatusDto) x.Status
            };
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] NewDeploymentDto newDeploymentDto)
        {
            var applications = _applicationService.GetApplications();
            var allApplicationsTrackingThisRepository = applications
                .SelectMany(
                    x => x.Images,
                    (app, img) =>
                        new
                        {
                            Image = img,
                            Application = app
                        }
                )
                .Where(x =>
                    x.Image.Repository.Equals(newDeploymentDto.Repository) &&
                    x.Image.Policy.IsMatch(newDeploymentDto.Tag)
                );

            var createdDeploymentsDto = new List<ApplicationDeploymentDto>();

            foreach (var item in allApplicationsTrackingThisRepository)
            {
                try
                {
                    var deployment = await _deploymentService.AddDeployment(item.Application, item.Image, newDeploymentDto.Tag);
                    createdDeploymentsDto.Add(ConvertFromModel(deployment));
                }
                catch
                {
                    // ignored
                }
            }
            
            return StatusCode(StatusCodes.Status201Created, createdDeploymentsDto);
        }
    }
}