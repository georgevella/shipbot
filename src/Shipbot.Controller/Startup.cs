using System.Reflection;
using System.Text.Json.Serialization;
using Mediator.Net;
using Mediator.Net.MicrosoftDependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Converters;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Shipbot.Applications;
using Shipbot.Applications.Slack;
using Shipbot.Contracts;
using Shipbot.Controller.Controllers;
using Shipbot.Controller.Core;
using Shipbot.Controller.Core.ApplicationSources;
using Shipbot.Controller.Core.ApplicationSources.Jobs;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Controller.Core.Registry;
using Shipbot.Controller.Core.Registry.Watcher;
using Shipbot.Deployments;
using Shipbot.JobScheduling;
using Shipbot.SlackIntegration;
using Shipbot.SlackIntegration.Commands;
using Slack.NetStandard.JsonConverters;

namespace Shipbot.Controller
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddNewtonsoftJson(
                    options =>
                    {
                        options.SerializerSettings.Converters.Add(new StringEnumConverter());
                        options.SerializerSettings.Converters.Add(new EventConverter());
                        options.SerializerSettings.Converters.Add(new CallbackEventConverter());
                        //options.SerializerSettings.Converters.Add(new NewtonsoftJsonSlackIncomingPayloadConverter());
                    }
                )
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
                // .AddJsonOptions(options =>
                // {
                //     options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                //     options.JsonSerializerOptions.Converters.Add(new SlackIncomingPayloadConverterFactory());
                // });

            services.AddHealthChecks();

            services.Configure<ShipbotConfiguration>(Configuration.GetSection("Shipbot"));
            services.Configure<SlackConfiguration>(Configuration.GetSection("Slack"));

            // application sources
            services.AddScoped<IApplicationSourceService, ApplicationSourceService>();
            services.AddTransient<GitRepositorySyncJob>();
            services.AddTransient<GitRepositoryCheckoutJob>();
            
            // applications
            services.AddSingleton<IApplicationStore, InMemoryApplicationStore>();
            services.AddScoped<IApplicationService, ApplicationService>();
            services.AddTransient<ISlackCommandHandler, GetCurrentApplicationTags>();

            // quartz
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddSingleton<IScheduler, DependencyInjectionQuartzScheduler>();
            services.AddSingleton<IJobFactory, DependencyInjectionQuartzJobFactory>();

            // container registry
            services.AddSingleton<RegistryClientPool>();
            services.AddSingleton<IRegistryWatcher, RegistryWatcher>();
            services.AddTransient<ContainerRegistryPollingJob>();


            services.RegisterShipbotDeploymentComponents();
            services.RegisterShipbotSlackIntegrationComponents();
            services.RegisterDbContext();

            services.AddTransient<IHostedService, OperatorStartup>();
            services.AddTransient<IHostedService, ShipbotApplicationsHostedService>();
            services.AddTransient<IHostedService, ContainerRegistryHostedService>();
            services.AddTransient<IHostedService, DeploymentSourcesHostedService>();

            services.RegisterMediator(
                new MediatorBuilder().RegisterHandlers(Assembly.GetExecutingAssembly())
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHealthChecks("/health");

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
