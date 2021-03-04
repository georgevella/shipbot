using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shipbot.Controller.Core.Configuration;

namespace Shipbot.Applications.Internal
{
    /// <summary>
    ///     Service that loads applications from configuration.
    /// </summary>
    public class ConfigurationSourceApplicationLoader : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<ShipbotConfiguration> _configuration;
        private readonly ILogger<ConfigurationSourceApplicationLoader> _log;

        public ConfigurationSourceApplicationLoader(
            IServiceProvider serviceProvider,
            IOptions<ShipbotConfiguration> configuration,
            ILogger<ConfigurationSourceApplicationLoader> log
            )
        {            
            _configuration = configuration;
            _log = log;
            _serviceProvider = serviceProvider;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var conf = _configuration.Value;

            using var scope = _serviceProvider.CreateScope();
            
            var applicationService = scope.ServiceProvider.GetService<IApplicationService>();

            foreach (var pair in conf.Applications)
            {
                var name = string.IsNullOrEmpty(pair.Value.Name) ? pair.Key : pair.Value.Name;
                var application = applicationService.AddApplication(name, pair.Value);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}