using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shipbot.Controller.Core.Configuration;

namespace Shipbot.SlackIntegration
{
    public class SlackStartup : IHostedService
    {
        private readonly IOptions<SlackConfiguration> _slackConfiguration;
        private readonly ISlackClient _slackClient;

        public SlackStartup(
            IOptions<SlackConfiguration> slackConfiguration,
            ISlackClient slackClient
            )
        {
            _slackConfiguration = slackConfiguration;
            _slackClient = slackClient;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _slackClient.Connect();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _slackClient.Dispose();

            return Task.CompletedTask;
        }
    }
}