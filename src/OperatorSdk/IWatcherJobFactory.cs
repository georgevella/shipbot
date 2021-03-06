using System.Threading;
using System.Threading.Tasks;
using k8s;

namespace OperatorSdk
{
    public interface IWatcherJobFactory
    {
        Task Start(Kubernetes client, CancellationToken token);
    }
    
    public interface INamespacedKubernetesResourceWatcherFactory
    {
        Task Start(string ns, CancellationToken token);
    }
}