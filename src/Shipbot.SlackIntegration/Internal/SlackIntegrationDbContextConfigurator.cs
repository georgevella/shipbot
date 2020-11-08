using Microsoft.EntityFrameworkCore;
using Shipbot.Data;

namespace Shipbot.SlackIntegration.Internal
{
    public class SlackIntegrationDbContextConfigurator : IDbContextConfigurator
    {
        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dao.SlackMessage>()
                .HasIndex(x => new {x.Timestamp, x.ChannelId});
        }
    }
}