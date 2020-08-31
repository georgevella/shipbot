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
using Shipbot.Applications;
using Shipbot.Contracts;
using Shipbot.Controller.Core.ApplicationSources;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Controller.Core.Configuration.Apps;
using Shipbot.Controller.Core.Configuration.Registry;

//using ArgoAutoDeploy.Core.K8s;
//using k8s;
// using ApplicationSourceRepository = Shipbot.Models.ApplicationSourceRepository;

namespace Shipbot.Controller.Core
{
    public class OperatorStartup : IHostedService
    {
        private readonly ILogger<OperatorStartup> _log;
        private readonly ConcurrentBag<Task> _watcherJobs = new ConcurrentBag<Task>();
        private readonly CancellationTokenSource _cancelSource;

        public OperatorStartup(
            ILogger<OperatorStartup> log, 
            IOptions<ShipbotConfiguration> configuration,
            IServiceProvider serviceProvider
        )
        {
            _log = log;

            _cancelSource = new CancellationTokenSource();
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            
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