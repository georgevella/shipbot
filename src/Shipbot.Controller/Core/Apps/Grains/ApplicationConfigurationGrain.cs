using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Configuration.ApplicationSources;
using Shipbot.Controller.Core.Configuration.Apps;
using Shipbot.Controller.Core.Utilities;
using Stateless.Graph;

namespace Shipbot.Controller.Core.Apps.Grains
{
    public class ApplicationConfigurationGrain : Grain, IApplicationConfigurationGrain
    {
        private readonly ILogger<ApplicationConfigurationGrain> _log;

        public ApplicationConfigurationGrain(ILogger<ApplicationConfigurationGrain> log)
        {
            _log = log;
        }
        
        public async Task Configure(ApplicationDefinition applicationDefinition)
        {
            using (_log.BeginShipbotLogScope(applicationDefinition.Name))
            {
                // setup application
                var applicationGrain = GrainFactory.GetApplication(applicationDefinition.Name);

                // setup environments
                _log.LogTrace("Configuring environments ...");
                foreach (var applicationDefinitionEnvironment in applicationDefinition.Environments)
                {
                    using (_log.BeginShipbotLogScope(applicationDefinition.Name, applicationDefinitionEnvironment.Key))
                    {
                        // setup application environment
                        var environmentGrain = GrainFactory.GetEnvironment(applicationDefinition.Name,
                            applicationDefinitionEnvironment.Key);
                        await environmentGrain.Configure(applicationDefinitionEnvironment.Value);   
                    }
                }
                _log.LogTrace("Configuring environments ... done");
                // State.Notifications = new NotificationSettings()
                // {
                //     Channels =
                //     {
                //         applicationDefinition.SlackChannel
                //     }
                // };
                
                _log.LogTrace("Refreshing environment state ...");
                foreach (var applicationDefinitionEnvironment in applicationDefinition.Environments)
                {
                    var applicationEnvironmentKey = new ApplicationEnvironmentKey(
                        applicationDefinition.Name,
                        applicationDefinitionEnvironment.Key);
                    
                    using (_log.BeginShipbotLogScope(applicationEnvironmentKey))
                    {
                        // setup application environment
                        var environmentGrain = GrainFactory.GetEnvironment(applicationEnvironmentKey);

                        // start listening to image tag notifications
                        _log.LogTrace("Start listening to image tag updates");
                        await environmentGrain.StartListeningToImageTagUpdates();

                        // check if we missed any tags and make sure all
                        _log.LogTrace("Check for misssing tags");
                        await environmentGrain.CheckForMissedImageTags();
                    }
                }
                _log.LogTrace("Refreshing environment state ... done");

                // setup deployment service
                var deploymentServiceGrain = GrainFactory.GetDeploymentServiceGrain(applicationDefinition.Name);
                await deploymentServiceGrain.Hello();
                
                
                // setup deployment sources, current image tags and apply them
                foreach (var applicationDefinitionEnvironment in applicationDefinition.Environments)
                {
                    var applicationEnvironmentKey = new ApplicationEnvironmentKey(
                        applicationDefinition.Name,
                        applicationDefinitionEnvironment.Key);
                    
                    var environmentGrain = GrainFactory.GetEnvironment(applicationEnvironmentKey);

                    
                    var deploymentSourceGrain = applicationDefinitionEnvironment.Value.Source.Type switch
                    {
                        ApplicationSourceType.Helm => GrainFactory.GetHelmDeploymentSourceGrain(applicationEnvironmentKey),
                        _ => throw new InvalidOperationException()
                    };

                    // we are reconfiguring the deployment source for this environment, stop any tracking currently running
                    await deploymentSourceGrain.StopTracking();

                    await deploymentSourceGrain.Configure(
                        applicationDefinitionEnvironment.Value.Source,
                        applicationEnvironmentKey
                    );

                    await deploymentSourceGrain.Checkout();
                    await deploymentSourceGrain.Refresh();
                    var currentTags = await deploymentSourceGrain.GetImageTags();
                    foreach (var keyValuePair in currentTags)
                    {
                        await environmentGrain.SetImageTag(keyValuePair.Key, keyValuePair.Value);
                    }

                    // activate the helm deployment repo watcher and updater.
                    await deploymentSourceGrain.StartTracking();    
                }
                
            }
        }
    }

    public interface IApplicationConfigurationGrain : IGrainWithGuidKey
    {
        Task Configure(ApplicationDefinition applicationDefinition);
    }
}