using Orleans;
using Shipbot.Controller.Core.Apps.Grains;
using Shipbot.Controller.Core.ContainerRegistry.Grains;


// ReSharper disable once CheckNamespace
namespace Orleans
{
    public partial class GrainFactoryExtensions
    {
        public static IElasticContainerRegistryWatcherGrain GetElasticContainerRegistryWatcher(this IGrainFactory grainfactory, string imageRepository)
        {
            return grainfactory.GetGrain<IElasticContainerRegistryWatcherGrain>(imageRepository);
        }
        
        public static IImageRepositoryGrain GetImageRepositoryGrain(this IGrainFactory grainFactory, string repository)
        {
            return grainFactory.GetGrain<IImageRepositoryGrain>(repository);
        }
    }
}