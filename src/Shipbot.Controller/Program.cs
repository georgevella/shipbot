using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;

namespace Shipbot.Controller
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
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
