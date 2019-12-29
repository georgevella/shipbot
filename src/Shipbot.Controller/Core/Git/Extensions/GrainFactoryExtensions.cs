using System;
using Orleans;
using Shipbot.Controller.Core.Git.Grains;

// ReSharper disable once CheckNamespace
namespace Orleans
{
    public static partial class GrainFactoryExtensions
    {
        private static readonly Guid Default = Guid.Parse("312AFEC3-9A04-438D-9884-45E48BE3EF77");
        
        public static IGitCredentialsRegistryGrain GetGitCredentialsRegistryGrain(this IGrainFactory grainFactory)
        {
            return grainFactory.GetGrain<IGitCredentialsRegistryGrain>(Default);
        }
        
    }
}