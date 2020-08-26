using Microsoft.Extensions.DependencyInjection;
using Shipbot.Deployments.Internals;

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
            services.AddScoped<IDeploymentWorkflowService, DeploymentWorkflowService>();
            services.AddDbContextConfigurator<DeploymentsDbContextConfigurator>();
            
            return services;
        }
    }
}