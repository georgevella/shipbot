using Microsoft.Extensions.Options;
using Shipbot.Controller.Core.Configuration;
using Slack.NetStandard;
using SlackAPI;

namespace Shipbot.SlackIntegration.Internal
{
    public class SlackClientWrapper : SlackTaskClient
    {
        public SlackClientWrapper(IOptions<SlackConfiguration> slackConfiguration) : base(slackConfiguration.Value.Token)
        {
            
        }
    }

    public class SlackApiClientWrapper : SlackWebApiClient
    {
        public SlackApiClientWrapper(IOptions<SlackConfiguration> slackConfiguration) : base(slackConfiguration.Value.Token)
        {
            
        }
    }
}