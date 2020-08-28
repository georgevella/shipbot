using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shipbot.SlackIntegration.Commands;
using Shipbot.SlackIntegration.Events;
using Shipbot.SlackIntegration.Events.EventHandlers;
using Shipbot.SlackIntegration.Internal;

namespace Shipbot.SlackIntegration
{
    public static class ShipbotSlackIntegrationServiceCollectionExtensions
    {
        public static IServiceCollection RegisterShipbotSlackIntegrationComponents(this IServiceCollection services)
        {
            services.AddTransient<IHostedService, SlackIntegrationHostedService>();

            services.AddSingleton<SlackClientWrapper>();
            
            services.AddScoped<ISlackClient, SlackClient>();

            services.AddTransient<ISlackEventHandler, AppMentionHandler>();
            services.AddScoped<ISlackEventDispatcher, SlackEventDispatcher>();

            services.AddScoped<ISlackCommandDispatcher, SlackCommandDispatcher>();

            services.AddDbContextConfigurator<SlackIntegrationDbContextConfigurator>();


            return services;
        }
    }
}