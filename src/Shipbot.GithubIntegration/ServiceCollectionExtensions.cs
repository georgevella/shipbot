using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shipbot.GithubIntegration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterGithubDataServices(this IServiceCollection services)
        {
            return services;
        }
        
        public static IServiceCollection RegisterShipbotSlackIntegrationComponents(this IServiceCollection services, IConfiguration configuration)
        {


            return services.RegisterGithubDataServices();
        }
    }
}