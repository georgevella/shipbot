using Newtonsoft.Json;

namespace Shipbot.Controller.Core.Apps.Models
{
    public class ApplicationEnvironmentImageKey
    {
        [JsonConstructor]
        public ApplicationEnvironmentImageKey(string repository, string imageTagValuePath)
        {
            Repository = repository;
            ImageTagValuePath = imageTagValuePath;
        }

        /// <summary>
        ///     URI of repository containing all images
        /// </summary>
        public string Repository { get; }

        /// <summary>
        ///     Path within the deployment source where we can find and store the image tag.
        /// </summary>
        public string ImageTagValuePath { get; }
    }
}