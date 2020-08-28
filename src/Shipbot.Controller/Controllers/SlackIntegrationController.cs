using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shipbot.Controller.Core;
using Shipbot.Controller.DTOs;
using Shipbot.Deployments;
using Shipbot.SlackIntegration.Events;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.EventsApi.CallbackEvents;
using Slack.NetStandard.JsonConverters;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Shipbot.Controller.Controllers
{
    [Route("slack/interaction/")]
    [ApiController]
    public class SlackIntegrationController : ControllerBase
    {
        private readonly ISlackEventDispatcher _slackEventDispatcher;
        private readonly IDeploymentService _deploymentService;
        private readonly IDeploymentQueueService _deploymentQueueService;
        private readonly IDeploymentNotificationService _deploymentNotificationService;

        public SlackIntegrationController(
            ISlackEventDispatcher slackEventDispatcher,
            IDeploymentService deploymentService,
            IDeploymentQueueService deploymentQueueService,
            IDeploymentNotificationService deploymentNotificationService
            )
        {
            _slackEventDispatcher = slackEventDispatcher;
            _deploymentService = deploymentService;
            _deploymentQueueService = deploymentQueueService;
            _deploymentNotificationService = deploymentNotificationService;
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
            var slackActionPayload = JsonConvert.DeserializeObject<BlockActionsPayload>(payload);
            
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

        [HttpPost("deployment-notification")]
        public async Task<ActionResult> ResendDeploymentNotification([FromForm(Name = "deploymentId")] Guid deploymentId)
        {
            var deployment = await _deploymentService.GetDeployment(deploymentId);
            await _deploymentNotificationService.CreateNotification(deployment);

            return Ok();
        }
    }
    
}