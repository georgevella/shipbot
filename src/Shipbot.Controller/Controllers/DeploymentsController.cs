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
            }

            return Ok(result);
        }
    }

    public class DeploymentDto
    {
        public List<PlannedDeploymentActionDto> DeploymentPlan { get; } = new List<PlannedDeploymentActionDto>();
        
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

    public class PlannedDeploymentActionDto
    {
        public string Environment { get; set; }
        
        public string Image { get; set; }
        
        public string CurrentTag { get; set; }
        
        public string TargetTag { get; set; }
        public string TagProperty { get; set; }
    }
}