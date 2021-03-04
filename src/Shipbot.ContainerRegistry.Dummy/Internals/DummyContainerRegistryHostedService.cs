using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shipbot.ContainerRegistry.Internals;
using Shipbot.Contracts;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Controller.Core.Configuration.Registry;

namespace Shipbot.ContainerRegistry.Dummy.Internals
{
    internal class DummyContainerRegistryHostedService : IHostedService
    {
        private readonly IOptions<ShipbotConfiguration> _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DummyContainerRegistryHostedService> _log;
        private readonly IRegistryClientPool _registryClientPool;

        public DummyContainerRegistryHostedService(
            IOptions<ShipbotConfiguration> configuration,
            IServiceProvider serviceProvider,
            ILogger<DummyContainerRegistryHostedService> log,
            IRegistryClientPool registryClientPool
            )
        { 
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _log = log;
            _registryClientPool = registryClientPool;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _log.LogTrace("DummyContainerRegistryHostedService::StartAsync() >>");

            using var scope = _serviceProvider.CreateScope();
            
            var conf = _configuration.Value;
            
            // register image repositories
            _log.LogInformation("Adding Container Registries");
            var dummyRegistryClients = conf.Registries
                .Where(x => x.Type == ContainerRegistryType.Dummy)
                .Select(settings => new DummyRegistryClient(settings.Dummy))
                .ToList();

            dummyRegistryClients.ForEach(x => _registryClientPool.AddClient(x));
            
            _log.LogTrace("DummyContainerRegistryHostedService::StartAsync() <<");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}