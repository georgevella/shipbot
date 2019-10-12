using System;
using CommandLine;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;
using Shipbot.Controller.Cmd;

namespace Shipbot.Controller
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = CreateWebHostBuilder(args);

            builder.Build().Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(
                    webBuilder =>
                    {
                        webBuilder
                            .UseStartup<Startup>()
                            .UseSerilog(
                                (hostingContext, loggerConfiguration) =>
                                {
                                    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
                                    if (hostingContext.HostingEnvironment.IsDevelopment())
                                    {
                                        loggerConfiguration
                                            .WriteTo.Console(
                                                outputTemplate:
                                                "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}"
                                            );
                                    }
                                    else
                                    {
                                        loggerConfiguration.WriteTo.Console(
                                            formatter: new JsonFormatter(renderMessage: true)
                                        );
                                    }
                                })
                            .UseShutdownTimeout(TimeSpan.FromSeconds(10));

                        CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args)
                            .WithParsed<CommandLineOptions>(opts =>
                            {
                                if (opts.ConfigFilePath != null)
                                {
                                    webBuilder.UseConfiguration(new ConfigurationBuilder()
                                        .AddJsonFile(opts.ConfigFilePath, false, true).Build());
                                }
                            });
                    }
                );
    }
}
