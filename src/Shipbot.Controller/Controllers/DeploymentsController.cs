using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shipbot.Controller.Core.Apps;
using Shipbot.Controller.Core.Deployments;
using Shipbot.Controller.Core.Models;

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

            foreach (var item in allApplicationsTrackingThisRepository)
            {
                await _deploymentService.AddDeploymentUpdate(item.Application, item.Image, newDeploymentDto.Tag);    
            }
            
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpGet("{application}")]
        public async Task<ActionResult<IEnumerable<ApplicationDeploymentDto>>> GetDeployments(string application)
        {
            var appModel = _applicationService.GetApplication(application);
            var deployments = await _deploymentService.GetDeployments(appModel);

            var result = deployments.Select(x => new ApplicationDeploymentDto()
            {
                Repository = x.Image.Repository,
                CurrentTag = x.CurrentTag,
                Tag = x.TargetTag,
                UpdatePath = x.Image.TagProperty.Path
            }).ToList();

            return Ok(result.AsEnumerable());
        }
    }

    public class NewDeploymentDto
    {
        public string Repository { get; set; }
        public string Tag { get; set; }
    }

    /// <summary>
    ///     Describes an appliation's deployment information
    /// </summary>
    public class ApplicationDeploymentDto : NewDeploymentDto
    {
        
        public string CurrentTag { get; set; }
        
        public string UpdatePath { get; set; }
    }
}