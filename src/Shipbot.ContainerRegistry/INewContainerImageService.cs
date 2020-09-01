using System.Collections.Generic;
using Shipbot.ContainerRegistry.Models;
using Shipbot.Models;

namespace Shipbot.ContainerRegistry
{
    public interface INewContainerImageService
    {
        ContainerImage GetLatestTagMatchingPolicy(
            IEnumerable<ContainerImage> images,
            ImageUpdatePolicy imagePolicy
            );

        IComparer<ContainerImage> GetComparer(ImageUpdatePolicy updatePolicy);
    }
}