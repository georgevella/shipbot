using System.Threading.Tasks;

namespace Shipbot.Controller.Core.Registry
{
    public interface IRegistryClientPool
    {
        Task<IRegistryClient> GetRegistryClientForRepository(string repository);
        void AddClient(IRegistryClient client);
    }
}