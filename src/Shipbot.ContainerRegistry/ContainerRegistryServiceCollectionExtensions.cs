using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Shipbot.ContainerRegistry.Internals;
using Shipbot.ContainerRegistry.Services;
using Shipbot.ContainerRegistry.Watcher;
using Shipbot.Contracts;

[assembly: InternalsVisibleTo("Shipbot.Tests")]

namespace Shipbot.ContainerRegistry
{
    public static class ContainerRegistryServiceCollectionExtensions
    {
        public static IServiceCollection RegisterShipbotContainerRegistryComponents(this IServiceCollection services)
        {
            services.AddSingleton<IRegistryClientPool, RegistryClientPool>();


            services.AddSingleton<IRegistryWatcher, ContainerRegistryTrackingService>();

            services.AddTransient<INewContainerImageService, NewContainerImageService>();
            
            services.AddTransient<ContainerRegistryPollingJob>();

            services.AddHostedService<ContainerRegistryHostedService>();

            return services;
        }
    }
}