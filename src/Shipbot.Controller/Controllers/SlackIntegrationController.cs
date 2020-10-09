using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shipbot.Deployments;
using Shipbot.SlackIntegration.Events;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.Interaction;

namespace Shipbot.Controller.Controllers
{
    [Route("slack/interaction/")]
    [ApiController]
    public class SlackIntegrationController : ControllerBase
    {
        private readonly ISlackEventDispatcher _slackEventDispatcher;
        private readonly IDeploymentService _deploymentService;
        private readonly IDeploymentQueueService _deploymentQueueService;

        public SlackIntegrationController(
            ISlackEventDispatcher slackEventDispatcher,
            IDeploymentService deploymentService,
            IDeploymentQueueService deploymentQueueService
        )
        {
            _slackEventDispatcher = slackEventDispatcher;
            _deploymentService = deploymentService;
            _deploymentQueueService = deploymentQueueService;
        }

        [HttpPost("events")]
        public async Task<ActionResult> Event([FromBody] Event payload)
        {
            return Ok(
                payload switch
                {
                    AppRateLimited appRateLimited => throw new NotImplementedException(),
                    EventCallback eventCallback => await _slackEventDispatcher.DispatchCallbackEvent(eventCallback.Event),
                    // EventCallback<TODO> eventCallback1 => throw new NotImplementedException(),
                    EventCallbackBase eventCallbackBase => throw new NotImplementedException(),
                    UrlVerification urlVerification => new
                    {
                        challenge = urlVerification.Challenge
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(payload))
                }
            );
        }

        [HttpPost("actions")]
        public async Task<ActionResult> Action([FromForm(Name = "payload")] string payload)
        {
            var slackActionPayload = JsonConvert.DeserializeObject<InteractionPayload>(payload);

            if (slackActionPayload is BlockActionsPayload blockActionsPayload)
            {
                if (blockActionsPayload.Actions.Any())
                {
                    var slackAction = blockActionsPayload.Actions.First();
                
                    if (slackAction.ActionId.Equals("deploy", StringComparison.OrdinalIgnoreCase))
                    {
                        if (Guid.TryParse(slackAction.Value, out var deploymentId))
                        {
                            var deployment = await _deploymentService.GetDeployment(deploymentId);
                            await _deploymentQueueService.EnqueueDeployment(deployment, TimeSpan.FromSeconds(5));
                        }
                    }
                }   
            }

            return Ok();
        }
    }
    
}