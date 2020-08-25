using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shipbot.SlackIntegration.Internal;

namespace Shipbot.SlackIntegration
{
    public static class ShipbotSlackIntegrationServiceCollectionExtensions
    {
        public static IServiceCollection RegisterShipbotSlackIntegrationComponents(this IServiceCollection services)
        {
            services.AddTransient<IHostedService, SlackIntegrationHostedService>();

            services.AddSingleton<SlackClientWrapper>();
            
            services.AddScoped<IDeploymentNotificationBuilder, DeploymentNotificationBuilder>();
            services.AddScoped<ISlackClient, SlackClient>();
            services.AddScoped<IDeploymentNotificationService, DeploymentNotificationService>();
            
            services.AddDbContextConfigurator<SlackIntegrationDbContextConfigurator>();


            return services;
        }
    }
}