using Microsoft.Extensions.DependencyInjection;

namespace Shipbot.Core
{
    public interface IShipbotModule
    {
        void RegisterServices(IServiceCollection services);

        void RegisterDataServices(IServiceCollection services);
    }
}