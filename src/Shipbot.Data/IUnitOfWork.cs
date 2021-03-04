using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Shipbot.Data
{
    public interface IUnitOfWork
    {
        Task Commit();
    }

    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ILogger<UnitOfWork> _log;
        private readonly ShipbotDbContext _dbContext;

        public UnitOfWork(
            ILogger<UnitOfWork> log,
            ShipbotDbContext dbContext
            )
        {
            _log = log;
            _dbContext = dbContext;
        }

        public async Task Commit()
        {
            var result = await _dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            if (_dbContext.ChangeTracker.HasChanges())
            {
                _log.LogWarning("Disposing unit of work, but db context has pending changes.");
            }
        }
    }
}