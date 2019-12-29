using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Deployments.GrainState;
using Shipbot.Controller.Core.Deployments.Models;

namespace Shipbot.Controller.Controllers
{ 
    [Route("api/deployments/{application}")]
    [ApiController]
    public class DeploymentsController : ControllerBase
    {
        private readonly IGrainFactory _grainFactory;

        public DeploymentsController(IGrainFactory grainFactory)
        {
            _grainFactory = grainFactory;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeploymentDto>>> Get(string application)
        {
            var result = new List<DeploymentDto>();
            
            var deploymentServiceGrain = _grainFactory.GetDeploymentServiceGrain(application);

            var deploymentIds = await deploymentServiceGrain.GetAllDeploymentIds();
            
            foreach (var deploymentKey in deploymentIds)
            {
                var deploymentDto = new DeploymentDto()
                {
                    Id = deploymentKey.DeploymentId
                };
                result.Add(deploymentDto);
                
                var deployment = _grainFactory.GetDeploymentGrain(deploymentKey);
                var deploymentActionIds = await deployment.GetDeploymentActionIds();

                foreach (var deploymentActionId in deploymentActionIds)
                {
                    var deploymentActionGrain = _grainFactory.GetDeploymentActionGrain(deploymentActionId);
                    var deploymentActionDto = new DeploymentUpdateDto()
                    {
                        Environment = deploymentActionId.Environment,
                        Image = deploymentActionId.ImageRepository,
                        TargetTag = deploymentActionId.TargetTag,
                        CurrentTag = (await deploymentActionGrain.GetCurrentTag()),
                        Status = (await deploymentActionGrain.GetStatus())
                    };
                    
                    deploymentDto.DeploymentUpdates.Add(deploymentActionDto);
                }
            }

            return Ok(result);
        }
    }

    public class DeploymentDto
    {
        public Guid Id { get; set; }
        
        public List<DeploymentUpdateDto> DeploymentUpdates { get; } = new List<DeploymentUpdateDto>();
    }

    public class DeploymentUpdateDto
    {
        public string Environment { get; set; }
        
        public string Image { get; set; }
        
        public string CurrentTag { get; set; }
        
        public string TargetTag { get; set; }

        public DeploymentActionStatus Status { get; set; }
    }
}