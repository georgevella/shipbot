using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Shipbot.Controller.Core.Apps.Models;
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
                    using (_log.BeginShipbotLogScope(applicationDefinition.Name, applicationDefinitionEnvironment.Key))
                    {
                        // setup application environment
                        var environmentGrain = GrainFactory.GetEnvironment(applicationDefinition.Name,
                            applicationDefinitionEnvironment.Key);
                        
                        _log.LogTrace("Start listening to image tag updates");

                        // start listening to image tag notifications
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
                // await WriteStateAsync();
            }
        }
    }

    public interface IApplicationConfigurationGrain : IGrainWithGuidKey
    {
        Task Configure(ApplicationDefinition applicationDefinition);
    }
}