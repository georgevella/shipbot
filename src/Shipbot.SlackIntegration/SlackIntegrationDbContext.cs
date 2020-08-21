using Microsoft.EntityFrameworkCore;

namespace Shipbot.SlackIntegration
{
    public class SlackIntegrationDbContext : DbContext
    {
        public DbSet<Dao.SlackMessage> SlackMessages { get; set; } = null!;
        
        public DbSet<Dao.DeploymentNotification> DeploymentNotifications { get; set; } = null!;

        public SlackIntegrationDbContext(DbContextOptions<SlackIntegrationDbContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dao.SlackMessage>()
                .HasIndex(x => new {x.Timestamp, x.ChannelId});
        }
    }
}