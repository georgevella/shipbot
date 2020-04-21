using System;
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

        public static IApplicationIndexGrain GetApplicationIndexGrain(this IGrainFactory grainFactory)
        {
            return grainFactory.GetGrain<IApplicationIndexGrain>(ApplicationIndexGrainKey);
        }        
        
        public static IApplicationConfigurationGrain GetApplicationConfigurationGrain(this IGrainFactory grainFactory)
        {
            return grainFactory.GetGrain<IApplicationConfigurationGrain>(ApplicationConfigurationGrainKey);
        }

        private static Guid ApplicationIndexGrainKey { get; } = Guid.Parse("{FFB716D4-124E-45E8-AD18-2ADE3A925BA2}");
        private static Guid ApplicationConfigurationGrainKey { get; } = Guid.Parse("{1A629FFE-D560-4CF3-AF4B-5B348181B180}");
    }
}