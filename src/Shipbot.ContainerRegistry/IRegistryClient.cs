using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shipbot.ContainerRegistry
{
    public interface IRegistryClient
    {
        Task<bool> IsKnownRepository(string repository);
        Task<IEnumerable<(string tag, DateTime createdAt)>> GetRepositoryTags(string repository);
    }
}