using System.Collections.Generic;
using System.Threading.Tasks;
using Shipbot.ContainerRegistry.Models;

namespace Shipbot.ContainerRegistry.Services
{
    public interface IContainerImageMetadataService
    {
        Task AddOrUpdate(ContainerImage containerImage);
        Task<IReadOnlyCollection<ContainerImage>> GetTagsForRepository(string repository);
        Task<ContainerImage> GetContainerImageByTag(string repository, string tag);
        Task<(bool success, ContainerImage image)> TryGetContainerImageByTag(string repository, string tag);
        Task AddOrUpdate(IEnumerable<ContainerImage> containerImages);
    }
}