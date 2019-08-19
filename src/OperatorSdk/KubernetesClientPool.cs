using System.Collections.Concurrent;
using ArgoAutoDeploy.Core.Configuration.K8s;
using k8s;
using Microsoft.Extensions.Logging;

namespace OperatorSdk
{
    public class KubernetesClientPool
    {
        private readonly KubernetesClientFactory _clientFactory;
        private readonly ILogger<KubernetesClientPool> _log;

        public KubernetesClientPool(
            KubernetesClientFactory clientFactory,
            ILogger<KubernetesClientPool> log
            
        )
        {
            _clientFactory = clientFactory;
            _log = log;
        }
        
        private readonly ConcurrentDictionary<KubernetesConnectionDetails, Kubernetes> _clientMap = new ConcurrentDictionary<KubernetesConnectionDetails, Kubernetes>();
        
        public Kubernetes GetConnection(KubernetesConnectionDetails connectionDetails)
        {
            _log.LogInformation("Opening connection to {name}", connectionDetails.Name);
            return _clientMap.GetOrAdd(connectionDetails, x => _clientFactory.Create(x));
        }

        public void Shutdown()
        {
            _log.LogInformation("Closing connections to k8s clusters ...");
            foreach (var clientMapValue in _clientMap.Values)
            {
                clientMapValue.Dispose();
            }
        }
    }
}