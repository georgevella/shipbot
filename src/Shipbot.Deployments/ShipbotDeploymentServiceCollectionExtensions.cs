using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Shipbot.Controller.Core.Deployments
{
    public static class ShipbotDeploymentServiceCollectionExtensions
    {
        public static IServiceCollection RegisterShipbotDeploymentComponents(this IServiceCollection services)
        {
            // deployment
            services.AddSingleton<IDeploymentQueueService, DeploymentQueueService>();
            services.AddSingleton<IDeploymentNotificationService, DeploymentNotificationService>();
            services.AddScoped<IDeploymentService, DeploymentService>();
            services.AddDbContext<DeploymentsDbContext>(
                builder => builder.UseNpgsql(
                    "Host=localhost;Database=postgres;Username=postgres;Password=password123"
                )
            );
            
            return services;
        }
    }
}