using Microsoft.Extensions.DependencyInjection;

namespace Shipbot.Deployments
{
    public static class ShipbotDeploymentServiceCollectionExtensions
    {
        public static IServiceCollection RegisterShipbotDeploymentComponents(this IServiceCollection services)
        {
            services.AddScoped<IDeploymentNotificationBuilder, DeploymentNotificationBuilder>();
            services.AddScoped<IDeploymentNotificationService, DeploymentNotificationService>();
            
            services.AddScoped<IDeploymentQueueService, DeploymentQueueService>();
            services.AddScoped<IDeploymentService, DeploymentService>();
            services.AddDbContextConfigurator<DeploymentsDbContextConfigurator>();
            
            return services;
        }
    }
}