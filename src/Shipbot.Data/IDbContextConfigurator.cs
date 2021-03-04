using Microsoft.EntityFrameworkCore;

namespace Shipbot.Data
{
    public interface IDbContextConfigurator
    {
        void OnModelCreating(ModelBuilder modelBuilder);
    }
}