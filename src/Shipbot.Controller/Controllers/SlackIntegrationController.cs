using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shipbot.Deployments;
using Shipbot.SlackIntegration;
using Shipbot.SlackIntegration.Dto.Slack.ExternalOptions;
using Shipbot.SlackIntegration.Interaction;
using Slack.NetStandard;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.Messages.Elements;

namespace Shipbot.Controller.Controllers
{
    [Route("slack/interaction/")]
    [ApiController]
    public class SlackIntegrationController : ControllerBase
    {
        private readonly ILogger<SlackIntegrationController> _log;
        private readonly ISlackEventDispatcher _slackEventDispatcher;
        private readonly ISlackShortcutInteractionDispatcher _slackShortcutInteractionDispatcher;
        private readonly ISlackInteractionActionDispatcher _slackInteractionActionDispatcher;
        private readonly IDeploymentService _deploymentService;
        private readonly IDeploymentQueueService _deploymentQueueService;
        private readonly ISlackApiClient _slackApiClient;
        private readonly ISlackExternalOptionsProvider _externalOptionsProvider;

        public SlackIntegrationController(
            ILogger<SlackIntegrationController> log,
            ISlackEventDispatcher slackEventDispatcher,
            ISlackShortcutInteractionDispatcher slackShortcutInteractionDispatcher,
            ISlackInteractionActionDispatcher slackInteractionActionDispatcher,
            IDeploymentService deploymentService,
            IDeploymentQueueService deploymentQueueService,
            ISlackApiClient slackApiClient,
            ISlackExternalOptionsProvider externalOptionsProvider
        )
        {
            _log = log;
            _slackEventDispatcher = slackEventDispatcher;
            _slackShortcutInteractionDispatcher = slackShortcutInteractionDispatcher;
            _slackInteractionActionDispatcher = slackInteractionActionDispatcher;
            _deploymentService = deploymentService;
            _deploymentQueueService = deploymentQueueService;
            _slackApiClient = slackApiClient;
            _externalOptionsProvider = externalOptionsProvider;
        }

        [HttpPost("events")]
        public async Task<ActionResult> Event([FromBody] Event payload)
        {
            async Task<object> ProcessEventCallBack(EventCallback ec)
            {
                await _slackEventDispatcher.Dispatch(ec.Event);
                return new object();
            }
            
            return Ok(
                payload switch
                {
                    AppRateLimited appRateLimited => throw new NotImplementedException(),
                    EventCallback eventCallback => ProcessEventCallBack(eventCallback),
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

        [HttpPost("external-options")]
        public async Task<ActionResult> ExternalOptions([FromForm(Name = "payload")] string payload)
        {
            var externalOptionsPayload = JsonConvert.DeserializeObject<ExternalOptionsPayload>(payload);

            async Task<ActionResult> ProcessBlockSuggestionPayload(BlockSuggestionPayload blockSuggestionPayload)
            {
                var options = await _externalOptionsProvider.Dispatch(blockSuggestionPayload);
                return Ok(new
                {
                    options
                });
            }

            return externalOptionsPayload switch
            {
                BlockSuggestionPayload blockSuggestionPayload => await ProcessBlockSuggestionPayload(
                    blockSuggestionPayload),
                // _ => throw new ArgumentOutOfRangeException(nameof(externalOptionsPayload))
                _ => Ok(
                    new
                    {
                        options = Enumerable.Empty<IOption>()
                    }
                )
            };
        }

        [HttpPost("actions")]
        public async Task<ActionResult> Action([FromForm(Name = "payload")] string payload)
        {
            var slackActionPayload = JsonConvert.DeserializeObject<InteractionPayload>(payload);
            
            _log.LogInformation($"Type: {slackActionPayload.Type}");

            async Task<ActionResult> ProcessBlockActionsPayload(BlockActionsPayload blockActionsPayload)
            {
                if (!blockActionsPayload.Actions.Any()) 
                    return Ok();
                
                foreach (var payloadAction in blockActionsPayload.Actions)
                {
                    await _slackInteractionActionDispatcher.Dispatch(new SlackAction(payloadAction, blockActionsPayload));
                }

                return Ok();
            }
            
            async Task<ActionResult> ProcessShortcutPayload(ShortcutPayload shortcutPayload)
            {
                await _slackShortcutInteractionDispatcher.Dispatch(shortcutPayload);
                return Ok();
            }

            async Task<ActionResult> ProcessViewSubmissionPayload(ViewSubmissionPayload viewSubmissionPayload)
            {
                return Ok();
            }
            
            return slackActionPayload switch 
            {
                ViewSubmissionPayload viewSubmissionPayload => await ProcessViewSubmissionPayload(viewSubmissionPayload),
                BlockActionsPayload blockActionsPayload => await ProcessBlockActionsPayload(blockActionsPayload),
                ShortcutPayload shortcutPayload => await ProcessShortcutPayload(shortcutPayload),
                _ => Ok()
            } ;
        }
    }
    
}