using System.Reflection;
using System.Text.Json.Serialization;
using Mediator.Net;
using Mediator.Net.MicrosoftDependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Controller.Core.ContainerRegistry;
using Shipbot.Controller.Core.ContainerRegistry.Clients;
using Shipbot.Controller.Core.ContainerRegistry.Watcher;
using Shipbot.Controller.Core.Jobs;
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
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                });
            
            services.AddHealthChecks();

            services.Configure<ShipbotConfiguration>(Configuration.GetSection("Shipbot"));
            services.Configure<SlackConfiguration>(Configuration.GetSection("Slack"));

//            services.AddSingleton<IApplicationService, ApplicationService>();
            services.AddSingleton<RegistryClientPool>();
            
//            // quartz
//            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
//            services.AddSingleton<IScheduler, DependencyInjectionQuartzScheduler>();
//            services.AddSingleton<IJobFactory, DependencyInjectionQuartzJobFactory>();

//            services.AddSingleton<IRegistryWatcher, RegistryWatcher>();
//            services.AddTransient<RegistryWatcherJob>();
//            services.AddSingleton<IRegistryWatcherStorage, RegistryWatcherStorage>();

            
            //services.AddTransient<IHostedService, OperatorStartup>();
            
            //services.AddTransient<IHostedService, SlackStartup>();
            //services.AddSingleton<ISlackClient, SlackClient>();

            services.AddControllers();
            
            services.RegisterMediator(
                new MediatorBuilder().RegisterHandlers(Assembly.GetExecutingAssembly())
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseHealthChecks("/health");

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
    
    
}
