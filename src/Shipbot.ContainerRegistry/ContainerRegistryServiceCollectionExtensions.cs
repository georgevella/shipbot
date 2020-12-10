using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Shipbot.ContainerRegistry.Internals;
using Shipbot.ContainerRegistry.Internals.Jobs;
using Shipbot.ContainerRegistry.Services;
using Shipbot.ContainerRegistry.Watcher;
using Shipbot.Contracts;

[assembly: InternalsVisibleTo("Shipbot.Tests")]

namespace Shipbot.ContainerRegistry
{
    public static class ContainerRegistryServiceCollectionExtensions
    {
        public static IServiceCollection RegisterContainerRegistryDataServices(this IServiceCollection services)
        {
            services.AddDbContextConfigurator<DbContextConfigurator>();
            return services;
        }
        
        public static IServiceCollection RegisterContainerRegistryComponents(this IServiceCollection services)
        {
            services.AddSingleton<IRegistryClientPool, RegistryClientPool>();


            services.AddSingleton<IRegistryWatcher, ContainerRegistryTrackingService>();

            services.AddTransient<INewContainerImageService, NewContainerImageService>();

            services.AddScoped<IContainerImageMetadataService, ContainerImageMetadataService>();
            
            services.AddTransient<ContainerRegistryPollingJob>();
            services.AddTransient<ApplicationContainerImagePollingJob>();

            services.AddHostedService<ContainerRegistryHostedService>();

            return services.RegisterContainerRegistryDataServices();
        }
    }
}