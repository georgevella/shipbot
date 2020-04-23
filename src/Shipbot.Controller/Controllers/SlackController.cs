using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Orleans;
using Shipbot.Controller.Core.Apps;
using Shipbot.Controller.Core.Deployments;
using Shipbot.Controller.Core.Slack.Models;
using Shipbot.Controller.Models.Slack;

namespace Shipbot.Controller.Controllers
{
    [Route("slack/interaction/")]
    [ApiController]
    public class SlackController : ControllerBase
    {
        private readonly IClusterClient _clusterClient;

        public SlackController(
            IClusterClient clusterClient
        )
        {
            _clusterClient = clusterClient;
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
                    var deployActionId = slackAction.Value;

                    var queueGrain  = _clusterClient.GetDeploymentQueueGrain();
                    await queueGrain.QueueDeploymentAction(deployActionId);

                    // var application = _applicationService.GetApplication(promoteActionDetails.Application);
                    // _deploymentService.PromoteDeployment(application, promoteActionDetails.ContainerRepository,
                    //     promoteActionDetails.TargetTag, promoteActionDetails.SourceEnvironment);
                }
            }
            
            return Ok();
        }
    }
}