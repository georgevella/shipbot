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
            using (_log.BeginShipbotLogScope(this.GetPrimaryKeyString()))
            {
                // setup application
                var applicationGrain = GrainFactory.GetApplication(applicationDefinition.Name);

                // setup environments
                foreach (var applicationDefinitionEnvironment in applicationDefinition.Environments)
                {
                    using (_log.BeginShipbotLogScope(this.GetPrimaryKeyString(), applicationDefinitionEnvironment.Key))
                    {
                        // setup application environment
                        var environmentGrain = GrainFactory.GetEnvironment(applicationDefinition.Name,
                            applicationDefinitionEnvironment.Key);
                        await environmentGrain.Configure(applicationDefinitionEnvironment.Value);   
                    }
                }

                // State.Notifications = new NotificationSettings()
                // {
                //     Channels =
                //     {
                //         applicationDefinition.SlackChannel
                //     }
                // };
                
                foreach (var applicationDefinitionEnvironment in applicationDefinition.Environments)
                {
                    using (_log.BeginShipbotLogScope(this.GetPrimaryKeyString(), applicationDefinitionEnvironment.Key))
                    {
                        // setup application environment
                        var environmentGrain = GrainFactory.GetEnvironment(applicationDefinition.Name,
                            applicationDefinitionEnvironment.Key);

                        // start listening to image tag notifications
                        await environmentGrain.StartListeningToImageTagUpdates();

                        // check if we missed any tags and make sure all
                        await environmentGrain.CheckForMissedImageTags();
                    }
                }

                // setup deployment service
                var deploymentServiceGrain = GrainFactory.GetDeploymentServiceGrain(this.GetPrimaryKeyString());
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