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
        private readonly IClusterClient _clusterClient;
        private readonly IGrainFactory _grainFactory;

        public DeploymentsController(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
            _grainFactory = clusterClient;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeploymentDto>>> Get(string application)
        {
            var result = new Dictionary<string, DeploymentDto>();
            
            var deploymentServiceGrain = _grainFactory.GetDeploymentServiceGrain(application);

            var deploymentIds = await deploymentServiceGrain.GetAllDeploymentIds();
            
            foreach (var deploymentKey in deploymentIds)
            {
                var deploymentDto = new DeploymentDto();
                result.Add(deploymentKey, deploymentDto);
                
                var deployment = _grainFactory.GetDeploymentGrain(deploymentKey);
                var deploymentActionIds = await deployment.GetDeploymentActionIds();

                foreach (var deploymentActionId in deploymentActionIds)
                {
                    var deploymentActionGrain = _grainFactory.GetDeploymentActionGrain(deploymentActionId);
                    var deploymentAction = await deploymentActionGrain.GetAction();
                    var deploymentActionDto = new DeploymentActionDto()
                    {
                        Environment = deploymentAction.ApplicationEnvironmentKey.Environment,
                        Image = deploymentAction.Image.Repository,
                        TargetTag = deploymentAction.TargetTag,
                        CurrentTag = (await deploymentActionGrain.GetCurrentTag()),
                        Status = (await deploymentActionGrain.GetStatus())
                    };

                    deploymentDto.DeploymentActions.Add(deploymentActionDto);
                }
            }

            return Ok(result);
        }
    }

    public class DeploymentDto
    {
        public List<DeploymentActionDto> DeploymentActions { get; } = new List<DeploymentActionDto>();
    }

    public class DeploymentActionDto
    {
        public string Environment { get; set; }
        
        public string Image { get; set; }
        
        public string CurrentTag { get; set; }
        
        public string TargetTag { get; set; }

        public DeploymentActionStatus Status { get; set; }
    }
}