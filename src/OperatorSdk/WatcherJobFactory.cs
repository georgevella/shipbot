using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using Microsoft.Extensions.Logging;

namespace OperatorSdk
{
    public class WatcherJobFactory<T> : IWatcherJobFactory
        where T : KubernetesObject
    {
        private readonly ILogger<WatcherJobFactory<T>> _log;
        private readonly IEnumerable<IWatcherEventHandler<T>> _handlers;
        private readonly CustomResourceAttribute _customResourceDetails;

        public WatcherJobFactory(ILogger<WatcherJobFactory<T>> log, IEnumerable<IWatcherEventHandler<T>> handlers)
        {
            _log = log;
            _handlers = handlers;
            _customResourceDetails = typeof(T).GetTypeInfo().GetCustomAttribute<CustomResourceAttribute>();
        }

        public async Task Start(Kubernetes client, CancellationToken token)
        {
            _log.LogInformation("Start() >>");
            
            var result = await client.ListNamespacedCustomObjectWithHttpMessagesAsync(
                _customResourceDetails.ApiGroup,
                _customResourceDetails.Version,
                "argo",
                _customResourceDetails.Plural, 
                watch: true,
                cancellationToken: token
            );
            
            
            using var watcher = result.Watch<T, object>(async (type, o) =>
            {
                foreach (var watcherEventHandler in _handlers)
                {
                    await watcherEventHandler.Handle(type, o);
                }
            });

            //token.Register(o => ((Watcher<T>) o).Dispose(), watcher);
            token.WaitHandle.WaitOne();
            
            _log.LogInformation("Start() <<");
        }
    }

    public class NamespacedResourceWatcherFactory<T> : INamespacedKubernetesResourceWatcherFactory
        where T : KubernetesObject
    {
        private readonly IEnumerable<IWatcherEventHandler<T>> _handlers;
        private readonly CustomResourceAttribute _customResourceDetails;
        private readonly GenericClient _genericClient;
        private readonly Kubernetes _client;

        public NamespacedResourceWatcherFactory(KubernetesClientConfiguration config, IEnumerable<IWatcherEventHandler<T>> handlers)
        {
            _customResourceDetails = typeof(T).GetTypeInfo().GetCustomAttribute<CustomResourceAttribute>();
            _genericClient = new GenericClient(config, _customResourceDetails.ApiGroup, _customResourceDetails.Version, _customResourceDetails.Plural);
            _client = new Kubernetes(config);
            _handlers = handlers;

            
        }
        public async Task Start(string ns, CancellationToken token)
        {
            
            var result = await _client.ListNamespacedCustomObjectWithHttpMessagesAsync(
                _customResourceDetails.ApiGroup,
                _customResourceDetails.Version,
                ns,
                _customResourceDetails.Plural, 
                watch: true,
                cancellationToken: token
            );

            var watcher = result.Watch<T, object>(
                async (type, o) =>
                {
                    foreach (var watcherEventHandler in _handlers)
                    {
                        await watcherEventHandler.Handle(type, o);
                    }
                }
            );

            token.Register(o => ((Watcher<T>) o).Dispose(), watcher);
        }
    }
}