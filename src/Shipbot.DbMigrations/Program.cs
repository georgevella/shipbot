using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shipbot.ContainerRegistry;
using Shipbot.Deployments;
using Shipbot.SlackIntegration;

namespace Shipbot.DbMigrations
{
    public class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(builder =>
                {
                    builder.AddEnvironmentVariables(prefix: "DOTNET_");
                    builder.AddEnvironmentVariables(prefix: "SHIPBOT_");
                })
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    var env = hostContext.HostingEnvironment;

                    builder
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((context, collection) =>
                {
                    collection.RegisterDeploymentDataServices();
                    collection.RegisterSlackIntegrationDataServices();
                    collection.RegisterContainerRegistryDataServices();
                    
                    collection.RegisterDbContext(
                        context.Configuration, 
                        builder => builder.MigrationsAssembly(typeof(Program).Assembly.FullName)
                        );
                });

    }
}
