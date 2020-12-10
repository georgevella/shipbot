using System.Threading.Tasks;
using Shipbot.Models;

namespace Shipbot.Contracts
{
    public interface IRegistryWatcher
    {
        Task StopWatchingImageRepository(string containerImageRepository);
        Task Shutdown();
        Task StartWatchingImageRepository(string containerImageRepository);

        Task<bool> IsWatched(string containerImageRepository);
    }
}