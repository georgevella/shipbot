using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Shipbot.ContainerRegistry.Models;
using Shipbot.Models;

namespace Shipbot.ContainerRegistry.Services
{
    internal class NewContainerImageService : INewContainerImageService
    {
        private readonly ILogger<NewContainerImageService> _log;

        public NewContainerImageService(ILogger<NewContainerImageService> log)
        {
            _log = log;
        }
        
        public ContainerImage GetLatestTagMatchingPolicy(
            IEnumerable<ContainerImage> images,
            ImageUpdatePolicy imagePolicy
        )
        {
            var matchingTags = images
                .Where(
                    tagDetails => imagePolicy.IsMatch(tagDetails.Tag)
                ).ToList();

            var latestImage = matchingTags
                .OrderBy(i => i.CreationDateTime, Comparer<DateTimeOffset>.Default)
                .Last();

            return latestImage;
        }

        public IComparer<ContainerImage> GetComparer(ImageUpdatePolicy updatePolicy)
        {
            return updatePolicy switch
            {
                GlobImageUpdatePolicy globImageUpdatePolicy =>
                    Comparer<ContainerImage>.Create(
                        (x, y) => x.Equals(y) ? 0 : x.CreationDateTime.CompareTo(y.CreationDateTime)),
                RegexImageUpdatePolicy regexImageUpdatePolicy =>
                    Comparer<ContainerImage>.Create(
                        (x, y) => x.Equals(y) ? 0 : x.CreationDateTime.CompareTo(y.CreationDateTime)),
                _ => throw new ArgumentOutOfRangeException(nameof(updatePolicy))
            };
        }
    }
}