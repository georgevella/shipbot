using System;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace Shipbot.JobScheduling
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection RegisterJobSchedulerServices(this IServiceCollection services, IConfiguration configuration)
        {
            // TODO: impelement job scheduler persistence
            // var connectionString = configuration?.GetSection("ConnectionStrings")?["Quartz"];
            //
            // if (connectionString == null)
            //     throw new InvalidOperationException("Quartz connection string not setup.");
            
            services.AddSingleton<ISchedulerFactory>(provider =>
            {
                var quartzSchedulerBuilder = SchedulerBuilder.Create();
                // quartzSchedulerBuilder.UsePersistentStore(
                //     options =>
                //     { 
                //         //options.UseProperties = true;
                //         options.UsePostgres(connectionString);
                //         options.UseJsonSerializer();
                //     }
                // );
                return quartzSchedulerBuilder.Build();
            } );
            services.AddSingleton<IScheduler, DependencyInjectionQuartzScheduler>();
            services.AddSingleton<IJobFactory, DependencyInjectionQuartzJobFactory>();

            return services;
        }
    }
}