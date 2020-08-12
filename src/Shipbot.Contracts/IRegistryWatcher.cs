using System.Threading.Tasks;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Registry.Watcher
{
    public interface IRegistryWatcher
    {
        Task StartWatchingImageRepository(Application application);
    }
}