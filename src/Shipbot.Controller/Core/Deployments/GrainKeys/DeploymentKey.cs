using System;
using System.Collections.Generic;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Deployments.Models
{
    /// <summary>
    ///     A reference to an image deployment.
    /// </summary>
    public class DeploymentKey
    {
        private sealed class DeploymentKeyEqualityComparer : IEqualityComparer<DeploymentKey>
        {
            public bool Equals(DeploymentKey x, DeploymentKey y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id == y.Id;
            }
        
            public int GetHashCode(DeploymentKey obj)
            {
                unchecked
                {
                    var hashCode = (obj.Id != null ? obj.Id.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }
        
        public static IEqualityComparer<DeploymentKey> EqualityComparer { get; } = new DeploymentKeyEqualityComparer();

        public static DeploymentKey Empty { get; } = new DeploymentKey(Guid.Empty);

        public Guid Id { get; }

        [JsonConstructor]
        public DeploymentKey(Guid id)
        {
            Id = id;
        }

        // public DeploymentKey() : this(Guid.NewGuid())
        // {
        //     
        // }
        
        public static implicit operator string(DeploymentKey deploymentKey)
        {
            return $"{deploymentKey.Id:N}";
        }
        
        public static implicit operator DeploymentKey(string deploymentKey)
        {
            return new DeploymentKey(Guid.Parse(deploymentKey));
        }
    }
    // public class DeploymentKey
    // {
    //     private sealed class DeploymentKeyEqualityComparer : IEqualityComparer<DeploymentKey>
    //     {
    //         public bool Equals(DeploymentKey x, DeploymentKey y)
    //         {
    //             if (ReferenceEquals(x, y)) return true;
    //             if (ReferenceEquals(x, null)) return false;
    //             if (ReferenceEquals(y, null)) return false;
    //             if (x.GetType() != y.GetType()) return false;
    //             return x.Application == y.Application &&
    //                    x.ImageRepository == y.ImageRepository &&
    //                    x.TargetTag == y.TargetTag &&
    //                    x.DeploymentId == y.DeploymentId;
    //         }
    //
    //         public int GetHashCode(DeploymentKey obj)
    //         {
    //             unchecked
    //             {
    //                 var hashCode = (obj.Application != null ? obj.Application.GetHashCode() : 0);
    //                 hashCode = (hashCode * 397) ^ (obj.ImageRepository != null ? obj.ImageRepository.GetHashCode() : 0);
    //                 hashCode = (hashCode * 397) ^ (obj.TargetTag != null ? obj.TargetTag.GetHashCode() : 0);
    //                 hashCode = (hashCode * 397) ^ (obj.DeploymentId.GetHashCode());
    //                 return hashCode;
    //             }
    //         }
    //     }
    //     
    //     public static IEqualityComparer<DeploymentKey> EqualityComparer { get; } = new DeploymentKeyEqualityComparer();
    //
    //     public DeploymentKey(string application, string imageRepository, string targetTag)
    //     {
    //         Application = application;
    //         ImageRepository = imageRepository;
    //         TargetTag = targetTag;
    //         DeploymentId = Guid.NewGuid();
    //     }
    //
    //     public DeploymentKey()
    //     {
    //         
    //     }
    //
    //     /// <summary>
    //     ///     The application that this deployment will update.
    //     /// </summary>
    //     public string Application { get; set; }
    //     
    //     public Guid DeploymentId { get; set; }
    //     
    //     /// <summary>
    //     ///    Location of repository where images are stored. 
    //     /// </summary>
    //     public string ImageRepository { get; set; }
    //     
    //     public string TargetTag { get; set; }
    //
    //     public static implicit operator string(DeploymentKey deploymentKey)
    //     {
    //         return $"{deploymentKey.Application}:{deploymentKey.ImageRepository}:{deploymentKey.TargetTag}:{deploymentKey.DeploymentId:N}";
    //     }
    //
    //     public static implicit operator DeploymentKey(string deploymentKey)
    //     {
    //         var parts = deploymentKey.Split(':');
    //         if (parts.Length != 4)
    //             throw new InvalidCastException();
    //
    //         return new DeploymentKey()
    //         {
    //             Application = parts[0],
    //             ImageRepository = parts[1],
    //             TargetTag = parts[2],
    //             DeploymentId = Guid.Parse(parts[3])
    //         };
    //     }
    // }
}