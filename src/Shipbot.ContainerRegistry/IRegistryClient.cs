using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shipbot.ContainerRegistry.Models;

namespace Shipbot.ContainerRegistry
{
    public interface IRegistryClient
    {
        Task<bool> IsKnownRepository(string repository);
        Task<IEnumerable<ContainerImage>> GetRepositoryTags(string repository);
        Task<ContainerImage> GetImage(string repository, string tag);
    }
}