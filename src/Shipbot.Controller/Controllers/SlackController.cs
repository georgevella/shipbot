using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        private readonly IDeploymentService _deploymentService;
        private readonly IApplicationService _applicationService;

        public SlackController(
            IDeploymentService deploymentService,
            IApplicationService applicationService
            )
        {
            _deploymentService = deploymentService;
            _applicationService = applicationService;
        }
        
        [HttpPost("actions")]
        public ActionResult Action([FromForm(Name = "payload")] string payload)
        {
            var slackActionPayload = JsonConvert.DeserializeObject<ActionPayload>(payload);

            if (slackActionPayload.Actions.Any())
            {
                var slackAction = slackActionPayload.Actions.First();

                if (slackAction.ActionId.Equals("promote", StringComparison.OrdinalIgnoreCase))
                {
                    var promoteActionDetails =
                        JsonConvert.DeserializeObject<SlackPromoteActionDetails>(slackAction.Value);

                    var application = _applicationService.GetApplication(promoteActionDetails.Application);
                    _deploymentService.PromoteDeployment(application, promoteActionDetails.ContainerRepository,
                        promoteActionDetails.TargetTag, promoteActionDetails.SourceEnvironment);
                }
            }
            
            return Ok();
        }
    }
}