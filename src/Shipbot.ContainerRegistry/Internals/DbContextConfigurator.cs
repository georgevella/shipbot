using Microsoft.EntityFrameworkCore;
using Shipbot.ContainerRegistry.Dao;
using Shipbot.Data;

namespace Shipbot.ContainerRegistry.Internals
{
    public class DbContextConfigurator : IDbContextConfigurator
    {
        public DbContextConfigurator()
        {
            
        }
        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContainerImageRepository>()
                .HasIndex(repository => repository.Name);

            modelBuilder.Entity<ContainerImageMetadata>()
                .HasIndex(x => x.RepositoryId)
                .IsUnique(false);
            modelBuilder.Entity<ContainerImageMetadata>()
                .HasIndex(x => new {x.RepositoryId, x.Hash})
                .IsUnique();
            
            modelBuilder.Entity<ContainerImageTag>()
                .HasIndex(x => new {x.RepositoryId, x.Tag})
                .IsUnique();
            
            modelBuilder.Entity<ContainerImageTag>()
                .HasIndex(x => new {x.RepositoryId, x.Tag})
                .IsUnique(false);

        }
    }
}