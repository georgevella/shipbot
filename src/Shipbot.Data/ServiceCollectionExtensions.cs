using Microsoft.EntityFrameworkCore;
using Shipbot.Data;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDbContextConfigurator<T>(this IServiceCollection serviceCollection)
            where T: class, IDbContextConfigurator
        {
            return serviceCollection.AddTransient<IDbContextConfigurator, T>();
        }

        public static IServiceCollection RegisterDbContext(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddDbContext<ShipbotDbContext>(
                builder => builder.UseNpgsql(
                    "Host=localhost;Database=postgres;Username=postgres;Password=password123"
                )
            );
        }
    }
}