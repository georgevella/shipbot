using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

        public static IServiceCollection RegisterDbContext(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddDbContext<ShipbotDbContext>(
                builder => builder
                    // .UseLoggerFactory(MyLoggerFactory)
                    // .EnableSensitiveDataLogging()
                    .UseNpgsql("Host=localhost;Database=postgres;Username=postgres;Password=password123")
            );
        }
    }
}