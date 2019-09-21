using System;
using CommandLine;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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
            
            CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed<CommandLineOptions>(opts =>
                {
                    if (opts.ConfigFilePath != null)
                    {
                        builder.UseConfiguration(new ConfigurationBuilder()
                            .AddJsonFile(opts.ConfigFilePath, false, true).Build());
                    }
                });
            
            builder.Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
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
    }
}
