using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Shipbot.Data
{
    public class ShipbotDbContext : DbContext
    {
        private readonly IEnumerable<IDbContextConfigurator> _configurators;


        public ShipbotDbContext(
            DbContextOptions<ShipbotDbContext> options, 
            IEnumerable<IDbContextConfigurator> configurators
            ) : base(options)
        {
            _configurators = configurators;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _configurators.ToList().ForEach( x=>x.OnModelCreating(modelBuilder));
        }
    }
}