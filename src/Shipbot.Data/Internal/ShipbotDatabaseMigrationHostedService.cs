using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shipbot.Data.Internal
{
    public class ShipbotDatabaseMigrationHostedService : IHostedService
    {
        private readonly ILogger<ShipbotDatabaseMigrationHostedService> _log;
        private readonly IServiceProvider _serviceProvider;

        public ShipbotDatabaseMigrationHostedService(
            ILogger<ShipbotDatabaseMigrationHostedService> log,
            IServiceProvider serviceProvider
            )
        {
            _log = log;
            _serviceProvider = serviceProvider;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var dbContext = scope.ServiceProvider.GetService<ShipbotDbContext>();
            
            _log.LogInformation("Checking database ...");
            await dbContext.Database.MigrateAsync(cancellationToken: cancellationToken);
            _log.LogInformation("Checking database ... done");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}