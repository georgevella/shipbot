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
using Quartz;
using Quartz.Impl.Matchers;
using Shipbot.Controller.Core.ApplicationSources;
using Shipbot.Controller.Core.Apps;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Controller.Core.Configuration.Apps;
using Shipbot.Controller.Core.Configuration.Registry;
using Shipbot.Controller.Core.Deployments;
using Shipbot.Controller.Core.Registry;
using Shipbot.Controller.Core.Registry.Ecr;
using Shipbot.Controller.Core.Registry.Watcher;
//using ArgoAutoDeploy.Core.K8s;
//using k8s;
using ApplicationSourceRepository = Shipbot.Controller.Core.Models.ApplicationSourceRepository;

namespace Shipbot.Controller.Core
{
    public class OperatorStartup : IHostedService
    {
        private readonly ILogger<OperatorStartup> _log;
        private readonly IOptions<ShipbotConfiguration> _configuration;
        private readonly RegistryClientPool _registryClientPool;
        private readonly IApplicationService _applicationService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IApplicationSourceService _applicationSourceService;
        private readonly IRegistryWatcher _registryWatcher;
        private readonly IDeploymentService _deploymentService;
        private readonly IScheduler _scheduler;
        private readonly NewImagesJobListener _newImagesJobListener;
        private readonly ConcurrentBag<Task> _watcherJobs = new ConcurrentBag<Task>();
        private readonly CancellationTokenSource _cancelSource;

        public OperatorStartup(
            ILogger<OperatorStartup> log, 
            IOptions<ShipbotConfiguration> configuration,
            RegistryClientPool registryClientPool,
            IApplicationService applicationService,
            IServiceProvider serviceProvider,
            IApplicationSourceService applicationSourceService,
            IRegistryWatcher registryWatcher,
            IDeploymentService deploymentService,
            IScheduler scheduler,
            NewImagesJobListener newImagesJobListener
            )
        {
            _log = log;
            _configuration = configuration;
            _registryClientPool = registryClientPool;
            _applicationService = applicationService;
            _serviceProvider = serviceProvider;
            _applicationSourceService = applicationSourceService;
            _registryWatcher = registryWatcher;
            _deploymentService = deploymentService;
            _scheduler = scheduler;
            _newImagesJobListener = newImagesJobListener;

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
            var trackedApplications = conf.Applications.Select(applicationDefinition => _applicationService.AddApplication( applicationDefinition ));

            foreach (var trackedApplication in trackedApplications)
            {
                await _applicationSourceService.AddApplicationSource(trackedApplication);
//                await _registryWatcher.StartWatchingImageRepository(trackedApplication);
            }
            
            _scheduler.ListenerManager.AddJobListener(_newImagesJobListener, GroupMatcher<JobKey>.GroupEquals("containerrepowatcher"));

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