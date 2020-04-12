using System;
using System.Net;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Shipbot.Controller.Cmd;
using Shipbot.Controller.Core;
using Shipbot.Controller.Core.Apps.Grains;
using Shipbot.Controller.Core.ContainerRegistry;
using Shipbot.Controller.Core.Deployments;
using Shipbot.Controller.Core.Deployments.GrainState;
using Shipbot.Controller.Core.Utilities.Eventing;

namespace Shipbot.Controller
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args);

            BuildOrleansSilo(hostBuilder);
            BuildOrleansClient(hostBuilder);
            SetupWebHosting(hostBuilder, args);

            hostBuilder.UseSerilog(
                (hostingContext, loggerConfiguration) =>
                {
                    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        loggerConfiguration
                            .Filter.ByExcluding( 
                                logEvent => logEvent.Properties["SourceContext"].ToString().Contains("Orleans") && 
                                            (logEvent.Exception != null || (logEvent.Level != LogEventLevel.Warning && logEvent.Level != LogEventLevel.Error && logEvent.Level != LogEventLevel.Fatal ))
                                            )
                            .WriteTo.Console(
                                outputTemplate:
                                "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: <{Application}:{Environment}> {Message:lj} {Properties:j}{NewLine}{Exception}"
                            );
                    }
                    else
                    {
                        loggerConfiguration.WriteTo.Console(
                            formatter: new JsonFormatter(renderMessage: true)
                        );
                    }
                });

            hostBuilder.ConfigureAppConfiguration(
                (context, builder) =>
                {
                    Parser.Default.ParseArguments<CommandLineOptions>(args)
                        .WithParsed<CommandLineOptions>(opts =>
                        {
                            if (opts.ConfigFilePath != null)
                            {
                                builder.AddJsonFile(opts.ConfigFilePath, false, true).Build();
                            }
                        });
                }
            );
            hostBuilder.ConfigureServices(services =>
            {
                services.AddHostedService<OperatorStartup>();
                //services.AddHostedService<SlackStartup>();
                services.Configure<ConsoleLifetimeOptions>(options => { options.SuppressStatusMessages = false; });
            });

            
            return hostBuilder.RunConsoleAsync();
        }

        static IHostBuilder BuildOrleansClient(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(services =>
            {
//                services.AddSingleton<IClusterClient>(
//                    provider =>
//                    {
//                        var clientBuilder = new ClientBuilder()
//                            .UseLocalhostClustering()
//                            .Configure<ClusterOptions>(options =>
//                            {
//                                options.ClusterId = "dev";
//                                options.ServiceId = "HelloWorldApp";
//                            })
//                            .Configure<ClientMessagingOptions>(options =>
//                            {
//                                options.ResponseTimeout = TimeSpan.FromSeconds(30);
//                            })
//                            .ConfigureApplicationParts(
//                                parts => parts.AddApplicationPart(typeof(IApplicationGrain).Assembly).WithReferences()
//                            );
//
//                        return clientBuilder.Build();
//                    }
//                );

                services.AddHostedService<OrleansClientStartup>();
            });
            return hostBuilder;
        }

        static IHostBuilder BuildOrleansSilo(IHostBuilder hostBuilder)
        {
            hostBuilder
                .UseOrleans((context, siloBuilder) =>
                {
                    siloBuilder
                        .UseLocalhostClustering()
                        .Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = "dev";
                            options.ServiceId = "HelloWorldApp";
                        })
                        .AddAdoNetGrainStorage(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, options =>
                        {
                            options.Invariant = "Npgsql";
                            options.UseJsonFormat = true;
                            options.ConfigureJsonSerializerSettings = settings =>
                            {
                                settings.ContractResolver = new DictionaryAsArrayResolver();
                            };
                            options.ConnectionString =
                                "User ID=postgres;Password=password123;Host=localhost;Database=shipbot;";
                        })                        
                        .AddAdoNetGrainStorage("PubSubStore", options =>
                        {
                            options.Invariant = "Npgsql";
                            options.UseJsonFormat = true;
                            options.ConfigureJsonSerializerSettings = settings =>
                            {
                                settings.ContractResolver = new DictionaryAsArrayResolver();
                            };
                            options.ConnectionString =
                                "User ID=postgres;Password=password123;Host=localhost;Database=shipbot;";
                        })
                        .UseAdoNetReminderService(options =>
                        {
                            options.Invariant = "Npgsql";
                            options.ConnectionString =
                                "User ID=postgres;Password=password123;Host=localhost;Database=shipbot;";
                        })
                        .AddSimpleMessageStreamProvider(ContainerRegistryStreamingConstants.ContainerRegistryStreamProvider, 
                            configurator =>
                        {
                            configurator.FireAndForgetDelivery = true;
                        })
                        .AddSimpleMessageStreamProvider(EventingStreamingConstants.EventHandlingStreamProvider,
                            configurator =>
                            {
                                configurator.FireAndForgetDelivery = true;
                            })
                        .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                        .ConfigureApplicationParts(parts =>
                            parts.AddApplicationPart(typeof(IApplicationGrain).Assembly).WithReferences()
                        )
                        .UseDashboard(options =>
                        {
                            options.BasePath = "orleans";
                        })
                        ;
                });

            return hostBuilder;
        }

        public static IHostBuilder SetupWebHosting(IHostBuilder hostBuilder, string[] args) =>
            hostBuilder
                .ConfigureWebHostDefaults(
                    webBuilder =>
                    {
                        webBuilder
                            .UseStartup<Startup>()
                            .UseShutdownTimeout(TimeSpan.FromSeconds(10));
                    }
                );
    }

    internal class GrainExtension
    {
    }

    internal class TestGrainExtension
    {
    }
}
