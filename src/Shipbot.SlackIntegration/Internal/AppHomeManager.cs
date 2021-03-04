using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Slack.NetStandard;
using Slack.NetStandard.Messages.Blocks;
using Slack.NetStandard.Objects;

namespace Shipbot.SlackIntegration.Internal
{
    public class AppHomeManager : IAppHomeManager
    {
        private readonly ILogger<AppHomeManager> _log;
        private readonly ISlackApiClient _slackApiClient;

        public AppHomeManager(
            ILogger<AppHomeManager> log,
            ISlackApiClient slackApiClient)
        {
            _log = log;
            _slackApiClient = slackApiClient;
        }

        public async Task Publish(string botUserId)
        {
            var response = await _slackApiClient.View.Publish(
                botUserId,
                new View
                {
                    Type = "home",
                    Blocks = new[]
                    {
                        new Section("test-app")
                        {
                            Fields = new List<TextObject>()
                            {
                                new MarkdownText("image1: *develop-456*")
                            }
                        }
                    }
                }
            );
            
            _log.LogInformation("Published");
        } 
    }

    public interface IAppHomeManager
    {
        Task Publish(string botUserId);
    }
}