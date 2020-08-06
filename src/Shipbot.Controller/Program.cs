using System;
using System.IO;
using System.Linq;
using CommandLine;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Json;
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
                    builder.ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        if (opts.ConfigFilePath != null)
                        {
                            config.AddJsonFile(opts.ConfigFilePath, false, true);
                        }

                        if (opts.ApplicationsFilePath?.Any() == true)
                        {
                            foreach (var path in opts.ApplicationsFilePath)
                            {
                                foreach (var file in Directory.GetFiles(Path.GetDirectoryName(path),
                                    Path.GetFileName(path)))
                                {
                                    switch (Path.GetExtension(file))
                                    {
                                        case ".yaml":
                                            config.AddYamlFile(file, true, true);
                                            break;
                                        case ".json":
                                            config.AddJsonFile(file, true, true);
                                            break;
                                    }
                                }
                            }
                        }
                    });
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