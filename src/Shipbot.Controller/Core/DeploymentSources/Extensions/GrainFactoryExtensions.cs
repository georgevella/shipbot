using Orleans;
using Shipbot.Controller.Core.Apps.Grains;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.DeploymentSources.Grains;

// ReSharper disable once CheckNamespace
namespace Orleans
{
    public static partial class GrainFactoryExtensions
    {
        public static IDeploymentSourceGrain GetHelmDeploymentSourceGrain(this IGrainFactory grainFactory, string application, string environment)
        {
            return grainFactory.GetGrain<IHelmDeploymentSourceGrain>(new ApplicationEnvironmentKey(application, environment));
        }
        
        public static IDeploymentSourceGrain GetHelmDeploymentSourceGrain(this IGrainFactory grainFactory, ApplicationEnvironmentKey applicationEnvironmentKey)
        {
            return grainFactory.GetGrain<IHelmDeploymentSourceGrain>(applicationEnvironmentKey);
        }
        
        public static IDeploymentSourceGrain GetHelmDeploymentSourceGrain(this IGrainFactory grainFactory, string name)
        {
            return grainFactory.GetGrain<IHelmDeploymentSourceGrain>(name);
        }
    }
}