using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Shipbot.Controller.Core.ContainerRegistry.Grains
{
    public class ContainerRegistryIndexGrain : Grain<HashSet<string>>, IContainerRegistryIndexGrain
    {
        public Task AddContainerRegistry(string containerRegistry)
        {
            State.Add(containerRegistry);

            return Task.CompletedTask;
        }

        public Task ActivateContainerRegistryTracking()
        {
            return Task.CompletedTask;
        }
    }

    public interface IContainerRegistryIndexGrain : IGrainWithGuidKey
    {
        Task AddContainerRegistry(string containerRegistry);

        Task ActivateContainerRegistryTracking();
    }
}