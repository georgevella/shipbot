using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Shipbot.Data;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        // public static readonly ILoggerFactory MyLoggerFactory
        //     = LoggerFactory.Create(builder =>
        //     {
        //         builder
        //             .AddFilter((category, level) =>
        //                 category == DbLoggerCategory.Database.Command.Name
        //                 && level == LogLevel.Information)
        //             .AddConsole();
        //     });
        //
        public static IServiceCollection AddDbContextConfigurator<T>(this IServiceCollection serviceCollection)
            where T: class, IDbContextConfigurator
        {
            return serviceCollection.AddTransient<IDbContextConfigurator, T>();
        }

        public static IServiceCollection RegisterDbContext(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
            serviceCollection.Add(
                new ServiceDescriptor(typeof(IEntityRepository<>), typeof(EntityRepository<>), ServiceLifetime.Scoped)
                );

            return serviceCollection.AddDbContext<ShipbotDbContext>(
                builder => builder
                    // .UseLoggerFactory(MyLoggerFactory)
                    // .EnableSensitiveDataLogging()
                    .UseNpgsql(configuration.GetShipbotConnectionString())
            );
        }
        
        public static IServiceCollection RegisterDbContext(this IServiceCollection serviceCollection, IConfiguration configuration, Action<NpgsqlDbContextOptionsBuilder> optionsBuilderFunc)
        {
            serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
            serviceCollection.Add(
                new ServiceDescriptor(typeof(IEntityRepository<>), typeof(EntityRepository<>), ServiceLifetime.Scoped)
            );

            return serviceCollection.AddDbContext<ShipbotDbContext>(
                builder => builder
                    // .UseLoggerFactory(MyLoggerFactory)
                    // .EnableSensitiveDataLogging()
                    .UseNpgsql(
                        configuration.GetShipbotConnectionString(), 
                        optionsBuilderFunc
                    )
            );
        }

        public static string GetShipbotConnectionString(this IConfiguration configuration)
        {
            return configuration.GetConnectionString("ShipbotDb");
        }
    }
}