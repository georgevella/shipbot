using Microsoft.Extensions.DependencyInjection;
using Shipbot.Deployments.Internals;
using Shipbot.Deployments.Internals.Jobs;
using Shipbot.Deployments.Slack;
using Shipbot.SlackIntegration.Interaction;

namespace Shipbot.Deployments
{
    public static class ShipbotDeploymentServiceCollectionExtensions
    {
        public static IServiceCollection RegisterDeploymentDataServices(this IServiceCollection services)
        {
            services.AddDbContextConfigurator<DeploymentsDbContextConfigurator>();
            return services;
        }
        
        public static IServiceCollection RegisterDeploymentComponents(this IServiceCollection services)
        {
            services.AddScoped<IDeploymentNotificationBuilder, DeploymentNotificationBuilder>();
            services.AddScoped<IDeploymentNotificationService, DeploymentNotificationService>();
            
            services.AddScoped<IDeploymentQueueService, DeploymentQueueService>();
            services.AddScoped<IDeploymentService, DeploymentService>();
            services.AddScoped<IDeploymentWorkflowService, DeploymentWorkflowService>();

            services.AddTransient<ISlackGlobalShortcutHandler, ManageDeploymentShortcutHandler>();
            
            services.AddTransient<ISlackActionHandler, DeployActionHandler>();
            services.AddTransient<ISlackActionHandler, AppNameSelectionActionHandler>();

            services.AddTransient<ApplicationUpdatesPollingJob>();
            services.AddTransient<ContainerImageRepositoryPollingJob>();

            services.AddHostedService<DeploymentsHostedService>();

            return services.RegisterDeploymentDataServices();
        }
    }
}