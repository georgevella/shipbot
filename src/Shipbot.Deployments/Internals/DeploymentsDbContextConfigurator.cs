using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Shipbot.Data;

namespace Shipbot.Deployments.Internals
{
    internal class DeploymentsDbContextConfigurator : IDbContextConfigurator
    {
        internal static T ParseEnumOrDefault<T>(string s, bool ignoreCase = true, T defaultValue = default)
            where T: struct, Enum
        {
            return Enum.TryParse(s, ignoreCase, out T result) ? result : defaultValue;
        }
        
        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dao.Deployment>()
                .Property(e => e.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => ParseEnumOrDefault(v , true, Dao.DeploymentStatus.Unknown)
                );
            
            modelBuilder.Entity<Dao.Deployment>()
                .Property(e => e.Type)
                .HasConversion(
                    v => v.ToString(),
                    v => ParseEnumOrDefault(v, true, Dao.DeploymentType.Unknown)
                );

            modelBuilder.Entity<Dao.Deployment>()
                .Property(e => e.Parameters)
                .HasConversion(
                    dictionary => JsonConvert.SerializeObject(dictionary, Formatting.None),
                    s => JsonConvert.DeserializeObject<Dictionary<string, string>>(s)
                )
                .Metadata.SetValueComparer(
                    new ValueComparer<Dictionary<string, string>>(
                        (x, y) => x.Comparer.Equals(y.Comparer) && x.Count == y.Count && x.Keys.Equals(y.Keys) &&
                                  x.Values.Equals(y.Values),
                        obj => obj.Count ^ (397 * obj.Keys.Sum(s => s.GetHashCode() * 487)) ^
                               (687 * obj.Values.Sum(s => s.GetHashCode() * 985))
                    )
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
            
            modelBuilder.Entity<Dao.DeploymentNotification>()
                .HasIndex( x=>x.DeploymentId );

            modelBuilder.Entity<Dao.DeploymentQueue>()
                .HasIndex(x => new
                {
                    x.ApplicationId,
                    x.AvailableDateTime,
                    x.AcknowledgeDateTime
                });
            
            modelBuilder.Entity<Dao.DeploymentQueue>()
                .HasIndex(x => new
                {
                    x.AvailableDateTime,
                    x.AcknowledgeDateTime
                });
        }
    }
}