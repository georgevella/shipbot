using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;
using Shipbot.Controller.Core.Apps.GrainState;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Configuration.ApplicationSources;
using Shipbot.Controller.Core.Configuration.Apps;
using Shipbot.Controller.Core.DeploymentSources.Models;
using Shipbot.Controller.Core.Utilities;
using ApplicationSourceRepository = Shipbot.Controller.Core.Configuration.ApplicationSources.ApplicationSourceRepository;

namespace Shipbot.Controller.Core.Apps.Grains
{
    public interface IApplicationGrain : IGrainWithStringKey
    {
        Task Configure(ApplicationDefinition applicationDefinition);
    }
    
    [StorageProvider()]
    public class ApplicationGrain : Grain<Application>, IApplicationGrain 
    {
        private readonly ILogger<ApplicationGrain> _log;

        public ApplicationGrain(ILogger<ApplicationGrain> log)
        {
            _log = log;
        }
        public override Task OnActivateAsync()
        {
            State.Notifications = new NotificationSettings();
            
            return base.OnActivateAsync();
        }

        public async Task Configure(ApplicationDefinition applicationDefinition)
        {
            using (_log.BeginShipbotLogScope(this.GetPrimaryKeyString()))
            {
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

                State.Notifications = new NotificationSettings()
                {
                    Channels =
                    {
                        applicationDefinition.SlackChannel
                    }
                };


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

                var deploymentServiceGrain = GrainFactory.GetDeploymentServiceGrain(this.GetPrimaryKeyString());
                await deploymentServiceGrain.Hello();
                await WriteStateAsync();
            }
        }
    }
}