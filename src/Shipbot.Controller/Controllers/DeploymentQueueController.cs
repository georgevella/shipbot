using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shipbot.Applications;
using Shipbot.Controller.Core.Deployments;
using Shipbot.Controller.DTOs;

namespace Shipbot.Controller.Controllers
{
    [Route("api/queue/")]
    [ApiController]
    public class DeploymentQueueController : ControllerBase
    {
        private readonly IApplicationService _applicationService;
        private readonly IDeploymentService _deploymentService;

        public DeploymentQueueController(
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
    }
}