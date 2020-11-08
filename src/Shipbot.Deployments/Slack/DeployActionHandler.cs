using System;
using System.Threading.Tasks;
using Shipbot.Applications;
using Shipbot.SlackIntegration.Interaction;
using Slack.NetStandard;
using Slack.NetStandard.Interaction;

namespace Shipbot.Deployments.Slack
{
    [SlackAction("deploy")]
    public class DeployActionHandler : ISlackActionHandler 
    {
        private readonly IDeploymentService _deploymentService;
        private readonly IDeploymentQueueService _deploymentQueueService;

        public DeployActionHandler(
            IDeploymentService deploymentService,
            IDeploymentQueueService deploymentQueueService
            )
        {
            _deploymentService = deploymentService;
            _deploymentQueueService = deploymentQueueService;
        }
        
        public async Task Process(SlackAction payload)
        {
            if (Guid.TryParse(payload.Value, out var deploymentId))
            {
                var deployment = await _deploymentService.GetDeployment(deploymentId);
                await _deploymentQueueService.EnqueueDeployment(deployment, TimeSpan.FromSeconds(5));
            }
        }
    }
    
    [SlackAction("app-name-selection")]
    public class AppNameSelectionActionHandler : ISlackActionHandler
    {
        private readonly ISlackApiClient _slackApiClient;
        private readonly IApplicationService _applicationService;

        public AppNameSelectionActionHandler(ISlackApiClient slackApiClient, IApplicationService applicationService)
        {
            _slackApiClient = slackApiClient;
            _applicationService = applicationService;
        }
        public async Task Process(SlackAction payload)
        {
            await _slackApiClient.View.UpdateByViewId(payload.Payload.View.ID,
                ViewBuilder.BuildManageDeploymentView(_applicationService, payload.Payload.View.State, true, true, false)
                );
        }
    }
}