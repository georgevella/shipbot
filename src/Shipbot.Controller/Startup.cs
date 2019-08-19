using System.Reflection;
using Mediator.Net;
using Mediator.Net.MicrosoftDependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Shipbot.Controller.Core;
using Shipbot.Controller.Core.ApplicationSources;
using Shipbot.Controller.Core.Apps;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Controller.Core.Jobs;
using Shipbot.Controller.Core.Registry;
using Shipbot.Controller.Core.Registry.Watcher;
using Shipbot.Controller.Core.Slack;
//using ArgoAutoDeploy.Core.Argo;
//using ArgoAutoDeploy.Core.Argo.Crd;
//using ArgoAutoDeploy.Core.K8s;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            
            services.AddHealthChecks();

            services.Configure<ShipbotConfiguration>(Configuration.GetSection("Shipbot"));
            services.Configure<SlackConfiguration>(Configuration.GetSection("Slack"));

            services.AddSingleton<IApplicationSourceService, ApplicationSourceService>();
            services.AddTransient<GitRepositorySyncJob>();
            services.AddTransient<GitRepositoryCheckoutJob>();
            
            services.AddSingleton<IApplicationService, ApplicationService>();
            services.AddSingleton<RegistryClientPool>();
            
            // quartz
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddSingleton<IScheduler, DependencyInjectionQuartzScheduler>();
            services.AddSingleton<IJobFactory, DependencyInjectionQuartzJobFactory>();

            services.AddSingleton<IRegistryWatcher, RegistryWatcher>();
            services.AddTransient<RegistryWatcherJob>();
            
            services.AddTransient<IHostedService, OperatorStartup>();
            
            
            services.AddTransient<IHostedService, SlackStartup>();
            services.AddSingleton<ISlackClient, SlackClient>();

            services.RegisterMediator(
                new MediatorBuilder().RegisterHandlers(Assembly.GetExecutingAssembly())
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
            app.UseMvc();
        }
    }
    
    
}
