using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shipbot.SlackIntegration.Commands;
using Shipbot.SlackIntegration.Events;
using Shipbot.SlackIntegration.Events.EventHandlers;
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
        
        public static IServiceCollection RegisterShipbotSlackIntegrationComponents(this IServiceCollection services)
        {
            services.AddTransient<IHostedService, SlackIntegrationHostedService>();

            // we make use of two libraries for slack comms
            services.AddSingleton<SlackClientWrapper>();
            services.AddSingleton<ISlackApiClient, SlackApiClientWrapper>();
            
            services.AddScoped<ISlackClient, SlackClient>();

            services.AddTransient<ISlackEventHandler, AppMentionHandler>();
            services.AddScoped<ISlackEventDispatcher, SlackEventDispatcher>();

            services.AddScoped<ISlackCommandDispatcher, SlackCommandDispatcher>();

            return services.RegisterSlackIntegrationDataServices();
        }
    }
}