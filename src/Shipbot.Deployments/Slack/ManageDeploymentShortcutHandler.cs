using System.Threading.Tasks;
using Shipbot.Applications;
using Shipbot.SlackIntegration.Interaction;
using Slack.NetStandard;
using Slack.NetStandard.Interaction;

namespace Shipbot.Deployments.Slack
{
    [SlackInteraction("manage_deployment")]
    public class ManageDeploymentShortcutHandler : ISlackGlobalShortcutHandler
    {
        private readonly ISlackApiClient _slackApiClient;
        private readonly IApplicationService _applicationService;

        public ManageDeploymentShortcutHandler(ISlackApiClient slackApiClient, IApplicationService applicationService)
        {
            _slackApiClient = slackApiClient;
            _applicationService = applicationService;
        }

        public async Task Process(ShortcutPayload shortcutPayload)
        {
            var res = await _slackApiClient.View.Open(shortcutPayload.TriggerId, ViewBuilder.BuildManageDeploymentView( _applicationService, null,true, false, false));
        } 
    }
}