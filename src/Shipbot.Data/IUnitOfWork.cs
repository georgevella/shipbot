using System.Threading.Tasks;

namespace Shipbot.Data
{
    public interface IUnitOfWork
    {
        
    }

    class UnitOfWork : IUnitOfWork
    {
        private readonly ShipbotDbContext _dbContext;

        public UnitOfWork(ShipbotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Commit()
        {
            var result = await _dbContext.SaveChangesAsync();
        }
    }
}