using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Shipbot.Models;

namespace Shipbot.ContainerRegistry.Services
{
    public class NewImageTagDetector : INewImageTagDetector
    {
        private readonly ILogger<NewImageTagDetector> _log;

        public NewImageTagDetector(ILogger<NewImageTagDetector> log)
        {
            _log = log;
        }
        
        public (bool newImageTagAvailable, string tag) GetLatestTag(
            IEnumerable<(string tag, DateTime createdAt)> tags, 
            string currentTag, 
            ImageUpdatePolicy imagePolicy
            )
        {
            var matchingTags = tags
                .Where(
                    tagDetails => imagePolicy.IsMatch(tagDetails.tag)
                )
                .ToDictionary(x => x.tag);

            var latestTag = matchingTags.Values
                .OrderBy(tuple => tuple.createdAt, Comparer<DateTime>.Default)
                .Last();

            if (latestTag.tag == currentTag)
            {
                _log.LogInformation("Latest image tag is applied to the deployment specs");
                return (false, string.Empty);
            }

            if (imagePolicy.IsGreaterThen(currentTag, latestTag.tag))
            {
                _log.LogInformation("Current tag is versioned higher than the tags presented.");
                return (false, string.Empty);
            }

            return (true, latestTag.tag);
        }
    }

    public interface INewImageTagDetector
    {
        (bool newImageTagAvailable, string tag) GetLatestTag(
            IEnumerable<(string tag, DateTime createdAt)> tags, 
            string currentTag, 
            ImageUpdatePolicy imagePolicy
            );
    }
}