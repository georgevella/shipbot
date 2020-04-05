using Orleans;
using Shipbot.Controller.Core.Apps.Grains;
using Shipbot.Controller.Core.Apps.Models;

// ReSharper disable once CheckNamespace
namespace Orleans
{
    public static partial class GrainFactoryExtensions
    {
        public static IApplicationGrain GetApplication(this IGrainFactory grainFactory, string name)
        {
            return grainFactory.GetGrain<IApplicationGrain>(name);
        }
        
        public static IApplicationEnvironmentGrain GetEnvironment(this IGrainFactory grainFactory, string application, string environment)
        {
            return grainFactory.GetGrain<IApplicationEnvironmentGrain>(new ApplicationEnvironmentKey(application, environment));
        }

        public static IApplicationEnvironmentGrain GetEnvironment(this IGrainFactory grainfactory, ApplicationEnvironmentKey key)
        {
            return grainfactory.GetGrain<IApplicationEnvironmentGrain>(key);
        }

        public static IImageRepositoryGrain GetContainerImage(this IGrainFactory grainFactory, string repository)
        {
            return grainFactory.GetGrain<IImageRepositoryGrain>(repository);
        }
    }
}