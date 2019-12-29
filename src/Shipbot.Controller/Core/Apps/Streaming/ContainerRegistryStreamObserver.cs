using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.ContainerRegistry.Models;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Apps.Streaming
{
    class ContainerRegistryStreamObserver : IAsyncObserver<ImageTag>
    {
        private readonly ILogger _log;
        private readonly Image _image;
        private readonly ImageUpdateSettings _imageUpdateSettings;
        private readonly ApplicationEnvironmentKey _applicationEnvironmentKey;
        private readonly IGrainFactory _grainFactory;
        
        public ContainerRegistryStreamObserver(
            ILogger log,
            Image image, 
            ImageUpdateSettings imageUpdateSettings,
            ApplicationEnvironmentKey applicationEnvironmentKey, 
            IGrainFactory grainFactory)
        {
            _log = log;
            _image = image;
            _imageUpdateSettings = imageUpdateSettings;
            _applicationEnvironmentKey = applicationEnvironmentKey;
            _grainFactory = grainFactory;
        }

        public async Task OnNextAsync(ImageTag item, StreamSequenceToken token = null)
        {
            using (_log.BeginScope(new Dictionary<string, object>()
            {
                {"Application", _applicationEnvironmentKey.Application},
                {"Environment", _applicationEnvironmentKey.Environment}
            }))
            {
                var environmentGrain = _grainFactory.GetEnvironment(_applicationEnvironmentKey);
                var currentTags = await environmentGrain.GetCurrentImageTags();

                if (_imageUpdateSettings.Policy.IsMatch(item.Tag))
                {
                    _log.Info("Received new image notification for {image}, with tag {tag}, for {application}::{env}",
                        _image,
                        item.Tag,
                        _applicationEnvironmentKey.Application,
                        _applicationEnvironmentKey.Environment);

                    var currentTag = currentTags[_image];

                    // handle the case where the current image tag does not match the policy specified (for example, manually updated or overridden)
                    // TODO: maybe we don't want to do this and we may need to put a flag or a facility to force
                    if ((!_imageUpdateSettings.Policy.IsMatch(currentTag)) ||
                        _imageUpdateSettings.Policy.IsGreaterThen(item.Tag, currentTag))
                    {
                        var deploymentServiceGrain =
                            _grainFactory.GetDeploymentServiceGrain(_applicationEnvironmentKey);
                        var deploymentKey = await deploymentServiceGrain.CreateNewImageDeployment(
                            _applicationEnvironmentKey.Environment,
                            _image,
                            item.Tag
                        );

                        if (deploymentKey != null)
                        {
                            // we have a deployment
                            var deploymentGrain = _grainFactory.GetDeploymentGrain(deploymentKey);
                            // await deploymentGrain.Deploy();

                            var firstDeploymentAction = (await deploymentGrain.GetDeploymentActionIds()).First();

                            var deploymentSourceGrain =
                                _grainFactory.GetHelmDeploymentSourceGrain(_applicationEnvironmentKey);

                            await deploymentSourceGrain.ApplyDeploymentAction(
                                firstDeploymentAction
                            );
                        }
                    }
                }
            }

        }

        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            _log.LogError(ex, "Error while processing new image deployment");
            return Task.CompletedTask;
        }
    }
}