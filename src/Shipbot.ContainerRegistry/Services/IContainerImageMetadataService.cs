using System.Collections.Generic;
using System.Threading.Tasks;
using Shipbot.ContainerRegistry.Models;

namespace Shipbot.ContainerRegistry.Services
{
    public interface IContainerImageMetadataService
    {
        Task AddOrUpdate(ContainerImage containerImage);
        Task<IEnumerable<ContainerImage>> GetTagsForRepository(string repository);
        Task<ContainerImage> GetContainerImageByTag(string repository, string tag);
        Task<(bool success, ContainerImage image)> TryGetContainerImageByTag(string repository, string tag);
    }
}