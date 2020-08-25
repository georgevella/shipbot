using System;
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
            
            // modelBuilder.Entity<Deployment>()
            //     .Property(e => e.Status)
            //     .HasConversion(
            //         v => v.ToString(),
            //         v => (DeploymentStatus) Enum.Parse(typeof(DeploymentStatus), v)
            //     );
            //
            // modelBuilder.Entity<Deployment>()
            //     .HasIndex(x => new
            //     {
            //         x.ApplicationId,
            //         x.ImageRepository,
            //         x.UpdatePath,
            //         x.CurrentImageTag,
            //         NewImageTag = x.TargetImageTag
            //     })
            //     .IsUnique();
            //
            // modelBuilder.Entity<SlackMessage>()
            //     .HasIndex(x => new {x.Timestamp, x.ChannelId});
        }
    }
}