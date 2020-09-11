using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Shipbot.Data;
using Shipbot.Deployments;
using Shipbot.SlackIntegration;

namespace Shipbot.DbMigrations
{
    public class BaseCommand<TCommandHandler> : Command
        where TCommandHandler : ICommandHandler
    {
        public BaseCommand(string name, string? alias = null, string? description = null) : base(name, description)
        {
            if (!string.IsNullOrWhiteSpace(alias))
            {
                AddAlias(alias);
            }
            
            Handler = CommandHandler.Create<IHost>( 
                async host =>
                {
                    await host.Services.GetService<TCommandHandler>().Run(CancellationToken.None);
                });
        }
    }
    
    public interface ICommandHandler
    {
        Task Run(CancellationToken cancellationToken);
    }
    
    public class MigrateCommandHandler : ICommandHandler
    {
        private readonly ShipbotDbContext _dbContext;

        public MigrateCommandHandler(
            ShipbotDbContext dbContext
        )
        {
            _dbContext = dbContext;
        }
        
        public Task Run(CancellationToken cancellationToken) => _dbContext.Database.MigrateAsync(cancellationToken);
    }
    
    public class InfoCommandHandler : ICommandHandler
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _hostEnvironment;

        public InfoCommandHandler(
            IConfiguration configuration,
            IHostEnvironment hostEnvironment
        )
        {
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
        }
        
        public Task Run(CancellationToken cancellationToken)
        {
            foreach (var item in _configuration.AsEnumerable())
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }
            
            Console.WriteLine("-----");
            Console.WriteLine($"Environment: {_hostEnvironment.EnvironmentName}");
            Console.WriteLine($"Application Name: {_hostEnvironment.ApplicationName}");
            Console.WriteLine($"Shipbot Connection string: {_configuration.GetShipbotConnectionString()}");
            
            return Task.CompletedTask;

        }
    }
    
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            static BaseCommand<MigrateCommandHandler> MigrateDatabase() =>
                new BaseCommand<MigrateCommandHandler>(
                    "migrate",
                    description:
                    "Applies pending database migrations."
                );
            
            static BaseCommand<InfoCommandHandler> Info() =>
                new BaseCommand<InfoCommandHandler>(
                    "info",
                    description:
                    "Applies pending database migrations."
                );
            
            var commandLineBuilder = new CommandLineBuilder()
                .AddCommand(MigrateDatabase())
                .AddCommand(Info())
                .AddOption(new Option(new[] {"--verbose"}))
                .UseDefaults()
                .UseHost(
                    CreateHostBuilder,
                    hostBuilder =>
                    {
                        hostBuilder
                            .ConfigureAppConfiguration((hostContext, builder) =>
                            {
                                var env = hostContext.HostingEnvironment;

                                builder
                                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                            })
                            .ConfigureServices((hostContext, services) =>
                            { ;
                                services.AddScoped<MigrateCommandHandler>();
                                services.AddScoped<InfoCommandHandler>();
                            });
                    }
                );

            var cli = commandLineBuilder.Build();
            var parseResults = cli.Parse(args);
            return await parseResults.InvokeAsync();
            
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseDefaultServiceProvider((context, options) => { })
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddConsole(options =>
                    {
                        options.DisableColors = false;
                        options.Format = ConsoleLoggerFormat.Default;
                    });
                })
                .ConfigureHostConfiguration(builder =>
                {
                    builder.AddEnvironmentVariables(prefix: "DOTNET_");
                    builder.AddEnvironmentVariables(prefix: "SHIPBOT_");
                })
                .ConfigureLogging((hostingContext, logging) =>
                    {
                        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                        logging.AddDebug();
                        logging.AddEventSourceLogger();
                        logging.AddFilter("Microsoft.Hosting", LogLevel.Error);
                    }
                )
                .ConfigureServices((context, collection) =>
                {
                    collection.RegisterShipbotDeploymentComponents();
                    collection.RegisterShipbotSlackIntegrationComponents();

                    collection.RegisterDbContext(context.Configuration);
                });
        
    }
}
