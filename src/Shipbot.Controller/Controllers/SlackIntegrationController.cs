using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shipbot.Controller.DTOs;
using Shipbot.Deployments;

namespace Shipbot.Controller.Controllers
{
    [Route("slack/interaction/")]
    [ApiController]
    public class SlackIntegrationController : ControllerBase
    {
        private readonly IDeploymentService _deploymentService;
        private readonly IDeploymentQueueService _deploymentQueueService;

        public SlackIntegrationController(
            IDeploymentService deploymentService,
            IDeploymentQueueService deploymentQueueService)
        {
            _deploymentService = deploymentService;
            _deploymentQueueService = deploymentQueueService;
        }
        
        [HttpPost("actions")]
        public async Task<ActionResult> Action([FromForm(Name = "payload")] string payload)
        {
            var slackActionPayload = JsonConvert.DeserializeObject<ActionPayload>(payload);

            if (slackActionPayload.Actions.Any())
            {
                var slackAction = slackActionPayload.Actions.First();
                
                if (slackAction.ActionId.Equals("deploy", StringComparison.OrdinalIgnoreCase))
                {
                    if (Guid.TryParse(slackAction.Value, out var deploymentId))
                    {
                        var deployment = await _deploymentService.GetDeployment(deploymentId);
                        await _deploymentQueueService.EnqueueDeployment(deployment, TimeSpan.FromSeconds(5));
                    }
                }
            }
            
            return Ok();
        }
    }
    
}