using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Shipbot.Controller.Core.Deployments.Models;

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

            await deployment.Deploy();
            
            // generate deployment DTO
            
            // TODO: extract this as an extension method
            var deploymentDto = new DeploymentDto();
            var deploymentActionIds = await deployment.GetDeploymentActionIds();

            foreach (var deploymentActionId in deploymentActionIds)
            {
                var deploymentActionGrain = _grainFactory.GetDeploymentActionGrain(deploymentActionId);
                var deploymentActionDto = new DeploymentActionDto()
                {
                    Environment = deploymentActionId.Environment,
                    Image = deploymentActionId.ImageRepository,
                    TargetTag = deploymentActionId.TargetTag,
                    CurrentTag = (await deploymentActionGrain.GetCurrentTag()),
                    Status = (await deploymentActionGrain.GetStatus())
                };

                deploymentDto.DeploymentActions.Add(deploymentActionDto);
            }

            var deploymentPlan = await deployment.GetDeploymentPlan();

            foreach (var plannedDeploymentAction in deploymentPlan)
            {
                deploymentDto.DeploymentPlan.Add(new PlannedDeploymentActionDto()
                {
                    Environment = plannedDeploymentAction.Environment,
                    Image = plannedDeploymentAction.Image.Repository,
                    TagProperty = plannedDeploymentAction.Image.TagProperty.Path,
                    CurrentTag = plannedDeploymentAction.CurrentTag,
                    TargetTag = plannedDeploymentAction.TargetTag
                });
            }

            return Ok(deploymentDto);
        }
    }

    public class DeploymentQueueEntryDto
    {
        public string Id { get; set; }
    }
}