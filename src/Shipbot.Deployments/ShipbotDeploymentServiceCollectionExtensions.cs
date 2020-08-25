using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shipbot.Contracts;
using Shipbot.SlackIntegration;

namespace Shipbot.Controller.Core.Deployments
{
    public static class ShipbotDeploymentServiceCollectionExtensions
    {
        public static IServiceCollection RegisterShipbotDeploymentComponents(this IServiceCollection services)
        {
            // deployment
            services.AddScoped<IDeploymentQueueService, DeploymentQueueService>();
            services.AddScoped<IDeploymentService, DeploymentService>();
            services.AddDbContextConfigurator<DeploymentsDbContextConfigurator>();



            return services;
        }
    }
}