using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.RestApiDto;

namespace Shipbot.Controller.Controllers
{
    [Route("api/deployment-queue/")]
    [ApiController]
    public class DeploymentQueueController : ControllerBase
    {
        private readonly IGrainFactory _grainFactory;

        public DeploymentQueueController(IClusterClient clusterClient)
        {
            _grainFactory = clusterClient;
        }
        
        [HttpPost()]
        public async Task<ActionResult<DeploymentDto>> SubmitDeploymentToQueue(DeploymentQueueItemDto deploymentQueueItem)
        {
            var deployment = _grainFactory.GetDeploymentGrain(deploymentQueueItem.DeploymentId);

            await deployment.SubmitNextDeploymentAction();
            
            // generate deployment DTO
            
            // TODO: extract this as an extension method
            var deploymentDto = new DeploymentDto(deploymentQueueItem.DeploymentId);
            var deploymentActionIds = await deployment.GetDeploymentActionIds();

            foreach (var deploymentActionId in deploymentActionIds)
            {
                var deploymentActionGrain = _grainFactory.GetDeploymentActionGrain(deploymentActionId);
                var deploymentAction = await deploymentActionGrain.GetAction();
                deploymentDto.Actions.Add(deploymentAction);
            }

            return Ok(deploymentDto);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DetailedDeploymentQueueItemDto>>> GetDeployment()
        {
            var deploymentQueue = _grainFactory.GetDeploymentQueueGrain();

            var queueItems =await deploymentQueue.GetQueue();

            var result = new List<DetailedDeploymentQueueItemDto>();

            foreach (var item in queueItems)
            {
                result.Add(new DetailedDeploymentQueueItemDto()
                {
                    Action = DeploymentActionType.Deploy, // TODO: this needs to be obtained from the queue
                    DeploymentId = item.Action,
                    Application = item.Application,
                    Environment = item.Environment,
                    Status = item.Status
                });
            }

            return result;
        }
    }

    public class DeploymentQueueItemDto
    {
        public string Application { get; set; }
        
        public DeploymentActionType Action { get; set; }
        
        public string DeploymentId { get; set; }
    }

    public class DetailedDeploymentQueueItemDto : DeploymentQueueItemDto
    {
        public string Environment { get; set; }
        public DeploymentActionStatus Status { get; set; }
    }

    public enum DeploymentActionType
    {
        Deploy,
        Revert
    }
}