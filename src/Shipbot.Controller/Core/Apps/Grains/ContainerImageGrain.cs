using System;
using System.Threading.Tasks;
using Orleans;
using Shipbot.Controller.Core.ContainerRegistry.Models;
using Shipbot.Controller.Core.Utilities;

namespace Shipbot.Controller.Core.Apps.Grains
{
    public class ContainerImageGrain : Grain, IContainerImageGrain
    {
        public Task SubmitNewImageTag(string tag)
        {
            var streamProvider = GetStreamProvider(Constants.InternalMessageStreamProvider);

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

    public interface IContainerImageGrain : IGrainWithStringKey
    {
        Task SubmitNewImageTag(string tag);
    }
}