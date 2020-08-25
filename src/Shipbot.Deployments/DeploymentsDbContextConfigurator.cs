using System;
using Microsoft.EntityFrameworkCore;
using Shipbot.Data;

namespace Shipbot.Controller.Core.Deployments
{
    public class DeploymentsDbContextConfigurator : IDbContextConfigurator
    {
        // public DbSet<Dao.Deployment> Deployments { get; set; } = null!;
        //
        // public DeploymentsDbContext(DbContextOptions<DeploymentsDbContext> options) : base(options)
        // {
        //     
        // }
        
        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dao.Deployment>()
                .Property(e => e.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (Dao.DeploymentStatus) Enum.Parse(typeof(Dao.DeploymentStatus), v)
                );

            modelBuilder.Entity<Dao.Deployment>()
                .HasIndex(x => new
                {
                    x.ApplicationId,
                    x.ImageRepository,
                    x.UpdatePath,
                    x.CurrentImageTag,
                    NewImageTag = x.TargetImageTag
                })
                .IsUnique();
        }
    }
}