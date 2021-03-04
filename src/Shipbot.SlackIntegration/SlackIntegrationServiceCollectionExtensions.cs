using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shipbot.Controller.Core.Configuration;
using Shipbot.SlackIntegration.Commands;
using Shipbot.SlackIntegration.Events;
using Shipbot.SlackIntegration.Events.EventHandlers;
using Shipbot.SlackIntegration.ExternalOptions;
using Shipbot.SlackIntegration.Interaction;
using Shipbot.SlackIntegration.Interaction.Dispatchers;
using Shipbot.SlackIntegration.Internal;
using Slack.NetStandard;

namespace Shipbot.SlackIntegration
{
    public static class ShipbotSlackIntegrationServiceCollectionExtensions
    {
        public static IServiceCollection RegisterSlackIntegrationDataServices(this IServiceCollection services)
        {
            services.AddDbContextConfigurator<SlackIntegrationDbContextConfigurator>();
            
            return services;
        }
        
        public static IServiceCollection RegisterShipbotSlackIntegrationComponents(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SlackConfiguration>(configuration.GetSection("Slack"));

            services.AddTransient<IHostedService, SlackIntegrationHostedService>();

            // we make use of two libraries for slack comms
            services.AddSingleton<SlackClientWrapper>();
            services.AddSingleton<ISlackApiClient, SlackApiClientWrapper>();
            

            services.AddTransient<IAppHomeManager, AppHomeManager>();
            services.AddScoped<ISlackClient, SlackClient>();

            // event handlers
            services.AddTransient<ISlackEventHandler, AppMentionHandler>();
            services.AddTransient<ISlackEventHandler, AppHomeOpenedEventHandler>();
            
            
            // dispatchers!
            services.AddScoped<ISlackEventDispatcher, SlackEventDispatcher>();
            services.AddScoped<ISlackShortcutInteractionDispatcher, ShortcutInteractionDispatcher>();
            services.AddScoped<ISlackCommandDispatcher, SlackCommandDispatcher>();
            services.AddScoped<ISlackExternalOptionsProvider, ExternalOptionsProvider>();
            services.AddScoped<ISlackInteractionActionDispatcher, InteractionActionDispatcher>();

            return services.RegisterSlackIntegrationDataServices();
        }
    }
}