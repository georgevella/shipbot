using System.Threading.Tasks;
using k8s;

namespace OperatorSdk
{
    public interface IWatcherEventHandler<in T>
        where T: KubernetesObject
    {
        Task Handle(WatchEventType eventType, T item);
    }
}