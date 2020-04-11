using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.RestApiDto;

namespace Shipbot.Controller.Controllers
{
    [Route("api/deployment-queue/{application}")]
    [ApiController]
    public class DeploymentQueueController : ControllerBase
    {
        private IGrainFactory _grainFactory;

        public DeploymentQueueController(IClusterClient clusterClient)
        {
            _grainFactory = clusterClient;
        }
        
        [HttpPost]
        public async Task<ActionResult<DeploymentDto>> SubmitDeploymentToQueue(string application, DeploymentQueueEntryDto deploymentQueueEntry)
        {
            var deployment = _grainFactory.GetDeploymentGrain(deploymentQueueEntry.Id);

            await deployment.SubmitNextDeploymentAction();
            
            // generate deployment DTO
            
            // TODO: extract this as an extension method
            var deploymentDto = new DeploymentDto(deploymentQueueEntry.Id);
            var deploymentActionIds = await deployment.GetDeploymentActionIds();

            foreach (var deploymentActionId in deploymentActionIds)
            {
                var deploymentActionGrain = _grainFactory.GetDeploymentActionGrain(deploymentActionId);
                var deploymentAction = await deploymentActionGrain.GetAction();
                deploymentDto.Actions.Add(deploymentAction);
            }

            return Ok(deploymentDto);
        }
    }

    public class DeploymentQueueEntryDto
    {
        public DeploymentActionType Action { get; set; }
        
        public string Id { get; set; }
    }

    public enum DeploymentActionType
    {
        Deploy,
        Revert
    }
}