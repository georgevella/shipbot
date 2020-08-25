using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shipbot.Controller.Core.Configuration;
using Shipbot.SlackIntegration.Internal;

namespace Shipbot.SlackIntegration
{
    public class SlackIntegrationHostedService : IHostedService
    {
        private readonly ILogger<SlackIntegrationHostedService> _log;
        private readonly IOptions<SlackConfiguration> _slackConfiguration;
        private readonly SlackClientWrapper _slackClientWrapper;

        public SlackIntegrationHostedService(
            ILogger<SlackIntegrationHostedService> log,
            IOptions<SlackConfiguration> slackConfiguration,
            SlackClientWrapper slackClientWrapper
            )
        {
            _log = log;
            _slackConfiguration = slackConfiguration;
            _slackClientWrapper = slackClientWrapper;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // var _timeout = _slackConfiguration.Value.Timeout;
            var loginResponse = await _slackClientWrapper.ConnectAsync();

            if (loginResponse.ok)
            {
                _log.LogInformation("Connection to slack established.");
            }
            else
            {
                throw new InvalidOperationException(loginResponse.error);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}