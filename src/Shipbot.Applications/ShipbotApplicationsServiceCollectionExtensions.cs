using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Shipbot.Applications.Internal;
using Shipbot.Applications.Slack;
using Shipbot.SlackIntegration.Commands;
using Shipbot.SlackIntegration.ExternalOptions;

[assembly: InternalsVisibleTo("Shipbot.Tests")]

namespace Shipbot.Applications
{
    public static class ShipbotApplicationsServiceCollectionExtensions
    {
        public static IServiceCollection RegisterApplicationManagementDataServices(this IServiceCollection services)
        {
            
            return services;
        }
        
        public static IServiceCollection RegisterApplicationManagementComponents(this IServiceCollection services)
        {
            // applications
            services.AddSingleton<IApplicationStore, InMemoryApplicationStore>();
            services.AddScoped<IApplicationService, ApplicationService>();
            services.AddTransient<ISlackCommandHandler, GetCurrentApplicationTags>();
            services.AddTransient<ISlackExternalOptionsSource, ApplicationIdsExternalOptionsSource>();
            services.AddTransient<ISlackExternalOptionsSource, ApplicationRepositoriesExternalOptionsSource>();
            services.AddTransient<IApplicationImageInstanceService, ApplicationImageInstanceService>(); 
            services.AddHostedService<ConfigurationSourceApplicationLoader>();

            return services.RegisterApplicationManagementDataServices();
        }
    }
}