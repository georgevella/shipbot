using System;
using System.Threading.Tasks;
using Orleans;
using Shipbot.Controller.Core.ContainerRegistry;
using Shipbot.Controller.Core.ContainerRegistry.Models;
using Shipbot.Controller.Core.Utilities;

namespace Shipbot.Controller.Core.Apps.Grains
{
    public class ImageRepositoryGrain : Grain, IImageRepositoryGrain
    {
        public Task SubmitNewImageTag(string tag)
        {
            var streamProvider = GetStreamProvider(Constants.ContainerRegistryStreamProvider);

            var containerImageStream = this.GetPrimaryKeyString().CreateGuidFromString();

            var stream = streamProvider.GetStream<ImageTag>(containerImageStream, Constants.ContainerRegistryQueueNamespace);

            return stream.OnNextAsync(new ImageTag()
            {
                Repository = this.GetPrimaryKeyString(),
                CreatedAt = DateTime.Now,
                Tag = tag
            });
        }
    }

    public interface IImageRepositoryGrain : IGrainWithStringKey
    {
        Task SubmitNewImageTag(string tag);
    }
}