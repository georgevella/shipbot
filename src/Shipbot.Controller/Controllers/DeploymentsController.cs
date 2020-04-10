using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Deployments.GrainState;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.RestApiDto;

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
            var result = new List<DeploymentDto>();
            
            var deploymentServiceGrain = _grainFactory.GetDeploymentServiceGrain(application);

            var deploymentIds = await deploymentServiceGrain.GetAllDeploymentIds();
            
            foreach (var deploymentKey in deploymentIds)
            {
                var deployment = _grainFactory.GetDeploymentGrain(deploymentKey);
                var deploymentActionIds = await deployment.GetDeploymentActionIds();
                
                
                var deploymentDto = new DeploymentDto(deploymentKey);
                foreach (var deploymentActionId in deploymentActionIds)
                {
                    var deploymentActionGrain = _grainFactory.GetDeploymentActionGrain(deploymentActionId);
                    var deploymentAction = await deploymentActionGrain.GetAction();
                    deploymentDto.Actions.Add(deploymentAction);
                }
                
                result.Add(deploymentDto);
            }

            return Ok(result);
        }
    }

}