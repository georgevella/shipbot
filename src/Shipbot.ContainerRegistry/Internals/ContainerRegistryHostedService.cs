using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shipbot.ContainerRegistry.Ecr;
using Shipbot.Contracts;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Controller.Core.Configuration.Registry;

namespace Shipbot.ContainerRegistry.Internals
{
    internal class ContainerRegistryHostedService: IHostedService
    {
        private readonly IOptions<ShipbotConfiguration> _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ContainerRegistryHostedService> _log;
        private readonly IRegistryClientPool _registryClientPool;

        public ContainerRegistryHostedService(
            IOptions<ShipbotConfiguration> configuration,
            IServiceProvider serviceProvider,
            ILogger<ContainerRegistryHostedService> log,
            IRegistryClientPool registryClientPool
            )
        { 
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _log = log;
            _registryClientPool = registryClientPool;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _log.LogTrace("ContainerRegistryHostedService::StartAsync() >>");

            using var scope = _serviceProvider.CreateScope();
            
            var conf = _configuration.Value;
            
            // register image repositories
            _log.LogInformation("Adding Container Registries");
            conf.Registries.ForEach(settings =>
            {
                _log.LogDebug($"Adding container registry {settings.Name}");
                switch (settings.Type)
                {
                    case ContainerRegistryType.DockerRegistry:
                        break;
                    
                    case ContainerRegistryType.Ecr:
                        _registryClientPool.AddClient(
                            new EcrClientFactory(scope.ServiceProvider).BuildClient(settings)
                        );
                        break;
                }
            } );
            
            // var applicationService = scope.ServiceProvider.GetService<IApplicationService>();
            // var registryWatcher = scope.ServiceProvider.GetService<IRegistryWatcher>();
            //
            // var trackedApplications = applicationService.GetApplications().ToList();
            //
            // foreach (var trackedApplication in trackedApplications)
            // {
            //     _log.LogInformation("Adding container registry tracking tracking for {Application}", trackedApplication.Name);
            //     await registryWatcher.StartWatchingImageRepository(trackedApplication);
            // }   
            
            _log.LogTrace("ContainerRegistryHostedService::StartAsync() <<");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var registryWatcher = scope.ServiceProvider.GetService<IRegistryWatcher>();
            return registryWatcher.Shutdown();
        }
    }
}