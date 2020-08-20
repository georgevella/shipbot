using System.Threading.Tasks;
using Shipbot.Models;

namespace Shipbot.Contracts
{
    public interface IRegistryWatcher
    {
        Task StartWatchingImageRepository(Application application);
    }
}