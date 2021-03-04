using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Shipbot.ContainerRegistry.Dummy.Internals;

[assembly: InternalsVisibleTo("Shipbot.Tests")]

namespace Shipbot.ContainerRegistry.Dummy
{
    public static class ContainerRegistryServiceCollectionExtensions
    {
        public static IServiceCollection RegisterDummyContainerRegistryComponents(this IServiceCollection services)
        {
            services.AddHostedService<DummyContainerRegistryHostedService>();

            return services;
        }
    }
}