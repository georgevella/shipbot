using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Quartz;
using Quartz.Impl.Matchers;
using Shipbot.Controller.Core.Apps;
using Shipbot.Controller.Core.Apps.Grains;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Controller.Core.Configuration.Apps;
using Shipbot.Controller.Core.Configuration.Registry;
using Shipbot.Controller.Core.ContainerRegistry;
using Shipbot.Controller.Core.ContainerRegistry.Clients;
using Shipbot.Controller.Core.ContainerRegistry.Clients.Ecr;
using Shipbot.Controller.Core.ContainerRegistry.Watcher;
using Shipbot.Controller.Core.Deployments;
using Shipbot.Controller.Core.DeploymentSources;
using Shipbot.Controller.Core.Git.Extensions;
//using ArgoAutoDeploy.Core.K8s;
//using k8s;
using ApplicationSourceRepository = Shipbot.Controller.Core.DeploymentSources.Models.ApplicationSourceRepository;

namespace Shipbot.Controller.Core
{
    public class OperatorStartup : IHostedService
    {
        private readonly ILogger<OperatorStartup> _log;
        private readonly IOptions<ShipbotConfiguration> _configuration;
        private readonly RegistryClientPool _registryClientPool;
        private readonly IServiceProvider _serviceProvider;
//        private readonly IScheduler _scheduler;
        private readonly IClusterClient _clusterClient;
        private readonly IGrainFactory _grainFactory;
        private readonly ConcurrentBag<Task> _watcherJobs = new ConcurrentBag<Task>();
        private readonly CancellationTokenSource _cancelSource;

        public OperatorStartup(
            ILogger<OperatorStartup> log, 
            IOptions<ShipbotConfiguration> configuration,
            RegistryClientPool registryClientPool,
            IServiceProvider serviceProvider,
//            IScheduler scheduler,
            IClusterClient clusterClient,
            IGrainFactory grainFactory
            )
        {
            _log = log;
            _configuration = configuration;
            _registryClientPool = registryClientPool;
            _serviceProvider = serviceProvider;
//            _scheduler = scheduler;
            _clusterClient = clusterClient;
            _grainFactory = grainFactory;

            _cancelSource = new CancellationTokenSource();
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            
            var conf = _configuration.Value;
            
            // register image repositories
            // conf.Registries.ForEach(settings =>
            // {
            //     switch (settings.Type)
            //     {
            //         case ContainerRegistryType.DockerRegistry:
            //             break;
            //         
            //         case ContainerRegistryType.Ecr:
            //             _registryClientPool.AddClient(
            //                 new EcrClientFactory(_serviceProvider).BuildClient(settings)
            //             );
            //             break;
            //     }
            // } );
            
            var credentialsRegistry = _grainFactory.GetGitCredentialsRegistryGrain();
            conf.GitCredentials.ForEach(c => credentialsRegistry.AddCredentials(c.Name, c.ConvertToGitCredentials()));
            
            // register applications
            foreach (var applicationDefinition in conf.Applications)
            {
                var grain = _grainFactory.GetApplicationConfigurationGrain();
                await grain.Configure(applicationDefinition);
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

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancelSource.Cancel();
            
            foreach (var task in _watcherJobs.ToArray())
            {
                task.Wait(cancellationToken);
            }

            return Task.CompletedTask;
        }
    }
}