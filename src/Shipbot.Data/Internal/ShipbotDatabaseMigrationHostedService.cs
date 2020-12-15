using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shipbot.Data.Internal
{
    public class ShipbotDatabaseMigrationHostedService : IHostedService
    {
        private readonly ILogger<ShipbotDatabaseMigrationHostedService> _log;
        private readonly ShipbotDbContext _dbContext;

        public ShipbotDatabaseMigrationHostedService(
            ILogger<ShipbotDatabaseMigrationHostedService> log,
            ShipbotDbContext dbContext)
        {
            _log = log;
            _dbContext = dbContext;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation("Checking database ...");
            await _dbContext.Database.MigrateAsync(cancellationToken: cancellationToken);
            _log.LogInformation("Checking database ... done");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}