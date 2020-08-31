using Microsoft.Extensions.DependencyInjection;
using Shipbot.ContainerRegistry.Internals;
using Shipbot.ContainerRegistry.Services;
using Shipbot.ContainerRegistry.Watcher;
using Shipbot.Contracts;

namespace Shipbot.ContainerRegistry
{
    public static class ContainerRegistryServiceCollectionExtensions
    {
        public static IServiceCollection RegisterShipbotContainerRegistryComponents(this IServiceCollection services)
        {
            services.AddSingleton<RegistryClientPool>();
            services.AddSingleton<IRegistryWatcher, RegistryWatcher>();

            services.AddTransient<INewImageTagDetector, NewImageTagDetector>();
            
            services.AddTransient<ContainerRegistryPollingJob>();

            services.AddHostedService<ContainerRegistryHostedService>();

            return services;
        }
    }
}