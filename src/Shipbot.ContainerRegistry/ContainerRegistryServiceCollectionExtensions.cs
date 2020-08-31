using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shipbot.Contracts;
using Shipbot.Controller.Core.Registry.Internals;
using Shipbot.Controller.Core.Registry.Services;
using Shipbot.Controller.Core.Registry.Watcher;
using Shipbot.Deployments;
using Shipbot.Deployments.Internals;

namespace Shipbot.Controller.Core.Registry
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