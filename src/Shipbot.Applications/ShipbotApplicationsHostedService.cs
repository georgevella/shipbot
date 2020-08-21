using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shipbot.Controller.Core.Configuration;

namespace Shipbot.Applications
{
    public class ShipbotApplicationsHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<ShipbotConfiguration> _configuration;

        public ShipbotApplicationsHostedService(
            IServiceProvider serviceProvider,
            IOptions<ShipbotConfiguration> configuration)
        {            
            _configuration = configuration;

            _serviceProvider = serviceProvider;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var conf = _configuration.Value;

            using var scope = _serviceProvider.CreateScope();
            
            var applicationService = scope.ServiceProvider.GetService<IApplicationService>();

            foreach (var applicationDefinition in conf.Applications)
            {
                applicationService.AddApplication(applicationDefinition.Value);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}