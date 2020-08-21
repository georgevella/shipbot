using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shipbot.Applications;
using Shipbot.Contracts;
using Shipbot.Controller.Core.Configuration;

namespace Shipbot.Controller.Core.ApplicationSources
{
    public class DeploymentSourcesHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<ShipbotConfiguration> _configuration;
        private readonly ILogger<DeploymentSourcesHostedService> _log;

        public DeploymentSourcesHostedService(
            IServiceProvider serviceProvider,
            IOptions<ShipbotConfiguration> configuration,
            ILogger<DeploymentSourcesHostedService> log
            )
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _log = log;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _log.LogTrace("DeploymentSourcesHostedService::StartAsync() >>");

            var conf = _configuration.Value;

            var map = conf.Applications.Values.ToDictionary(x => x.Name);
            
            using var scope = _serviceProvider.CreateScope();
            
            var applicationService = scope.ServiceProvider.GetService<IApplicationService>();
            var applicationSourceService = scope.ServiceProvider.GetService<IApplicationSourceService>();
            
            var trackedApplications = applicationService.GetApplications().ToList();
            foreach (var trackedApplication in trackedApplications)
            {
                _log.LogInformation("Adding deployment source tracking for {Application}", trackedApplication.Name);
                await applicationSourceService.AddApplicationSource(trackedApplication.Name, map[trackedApplication.Name].Source);
            }
            
            _log.LogTrace("DeploymentSourcesHostedService::StartAsync() <<");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}