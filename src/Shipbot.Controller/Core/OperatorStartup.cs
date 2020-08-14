using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shipbot.Controller.Core.ApplicationSources;
using Shipbot.Controller.Core.Apps;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Controller.Core.Configuration.Apps;
using Shipbot.Controller.Core.Configuration.Registry;
using Shipbot.Controller.Core.Registry;
using Shipbot.Controller.Core.Registry.Ecr;
//using ArgoAutoDeploy.Core.K8s;
//using k8s;
using ApplicationSourceRepository = Shipbot.Models.ApplicationSourceRepository;

namespace Shipbot.Controller.Core
{
    public class OperatorStartup : IHostedService
    {
        private readonly ILogger<OperatorStartup> _log;
        private readonly IOptions<ShipbotConfiguration> _configuration;
        private readonly RegistryClientPool _registryClientPool;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentBag<Task> _watcherJobs = new ConcurrentBag<Task>();
        private readonly CancellationTokenSource _cancelSource;

        public OperatorStartup(
            ILogger<OperatorStartup> log, 
            IOptions<ShipbotConfiguration> configuration,
            RegistryClientPool registryClientPool,
            IServiceProvider serviceProvider
            )
        {
            _log = log;
            _configuration = configuration;
            _registryClientPool = registryClientPool;
            _serviceProvider = serviceProvider;

            _cancelSource = new CancellationTokenSource();
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var conf = _configuration.Value;
            
            // register image repositories
            conf.Registries.ForEach(settings =>
            {
                switch (settings.Type)
                {
                    case ContainerRegistryType.DockerRegistry:
                        break;
                    
                    case ContainerRegistryType.Ecr:
                        _registryClientPool.AddClient(
                            new EcrClientFactory(_serviceProvider).BuildClient(settings)
                        );
                        break;
                }
            } );
            
            
            // register applications
            using var scope = _serviceProvider.CreateScope();
            
            var applicationService = scope.ServiceProvider.GetService<IApplicationService>();
                
            var trackedApplications = conf.Applications.Select(applicationDefinition => applicationService.AddApplication( applicationDefinition.Value ));

            foreach (var trackedApplication in trackedApplications)
            {
                await applicationService.StartTrackingApplication(trackedApplication);
            }


            //            // start watching argo applications
//            foreach (var connectionDetails in conf.Kubernetes)
//            {
//                var client = _clientPool.GetConnection(connectionDetails);
//                foreach (var customResourceWatcher in _watchers)
//                {
//                    _log.LogInformation("Starting watcher ...");
//                    
//                    var task = Task.Factory.StartNew(
//                        c => customResourceWatcher.Start((Kubernetes) c, _cancelSource.Token), 
//                        client, 
//                        _cancelSource.Token, 
//                        TaskCreationOptions.LongRunning,
//                        TaskScheduler.Default
//                    );
//                    
//                    _watcherJobs.Add(task.Unwrap());
//                    
//                    _log.LogInformation("Starting watcher ... done, next");
//                }
//            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancelSource.Cancel();
            
            foreach (var task in _watcherJobs.ToArray())
            {
                task.Wait();
            }

//            _clientPool.Shutdown();
        }
    }
}