using System.Threading.Tasks;

namespace Shipbot.ContainerRegistry
{
    public interface IRegistryClientPool
    {
        Task<IRegistryClient> GetRegistryClientForRepository(string repository);
        void AddClient(IRegistryClient client);
    }
}