using System.Threading.Tasks;
using Shipbot.ContainerRegistry.Models;

namespace Shipbot.ContainerRegistry.Dummy
{
    public interface IDummyImageRegistryStore
    {
        Task AddContainerImage(ContainerImage containerImage);
    }
}