using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shipbot.Applications;
using Shipbot.Controller.Core.Deployments;
using Shipbot.Controller.DTOs;
using Shipbot.Models;

namespace Shipbot.Controller.Controllers
{
    [Route("api/queue/")]
    [ApiController]
    public class DeploymentQueueController : ControllerBase
    {
        private readonly IApplicationService _applicationService;
        private readonly IDeploymentService _deploymentService;
        private readonly IDeploymentQueueService _deploymentQueueService;

        public DeploymentQueueController(
            IApplicationService applicationService,
            IDeploymentService deploymentService,
            IDeploymentQueueService deploymentQueueService
        )
        {
            _applicationService = applicationService;
            _deploymentService = deploymentService;
            _deploymentQueueService = deploymentQueueService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationDeploymentDto>>> GetQueue()
        {
            var d = await _deploymentQueueService.GetPendingDeployments();
            return Ok(d.ToList());
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
    }
}