using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shipbot.Controller.Core.Deployments;
using Shipbot.Data;
using Shipbot.SlackIntegration;

namespace Shipbot.DbMigrations
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, collection) =>
                {
                    collection.RegisterShipbotDeploymentComponents();
                    collection.RegisterShipbotSlackIntegrationComponents();
                    
                    collection.AddDbContext<ShipbotDbContext>(Setup);
                });

        private static void Setup(DbContextOptionsBuilder builder)
        {
            builder.UseNpgsql(
                "Host=localhost;Database=postgres;Username=postgres;Password=password123",
                b => b.MigrationsAssembly(typeof(Program).Assembly.FullName)
            );
        }
    }
}
