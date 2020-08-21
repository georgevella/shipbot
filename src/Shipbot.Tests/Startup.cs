using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shipbot.Applications;
using Shipbot.Contracts;
using Shipbot.Controller.Core.ApplicationSources;
using Shipbot.Controller.Core.Deployments;
using Shipbot.Controller.Core.Registry.Watcher;
using Shipbot.SlackIntegration;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace Shipbot.Tests
{
    public class Startup
    {
        public void Configure(ILoggerFactory loggerFactory, ITestOutputHelperAccessor accessor) =>
            loggerFactory.AddProvider(new XunitTestOutputLoggerProvider(accessor));

        public void ConfigureServices(IServiceCollection services)
        {
            var slackClient = new Mock<ISlackClient>(MockBehavior.Default).Object;
            services.AddSingleton<ISlackClient>(slackClient);

            var applicationSourceService = new Mock<IApplicationSourceService>(MockBehavior.Default).Object;
            services.AddSingleton<IApplicationSourceService>(applicationSourceService);

            var registryWatcher = new Mock<IRegistryWatcher>(MockBehavior.Default).Object;
            services.AddSingleton<IRegistryWatcher>(registryWatcher);
                
            
            services.AddTransient<IApplicationService, ApplicationService>();
            services.AddTransient<IDeploymentService, DeploymentService>();
        } 
        
    }
}