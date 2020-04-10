using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Deployments.Models;

namespace Shipbot.Controller.Core.Deployments.GrainKeys
{
    /// <summary>
    ///     A reference to an image deployment.
    /// </summary>
    public class DeploymentActionKey
    {
        private sealed class DeploymentKeyEqualityComparer : IEqualityComparer<DeploymentActionKey>
        {
            public bool Equals(DeploymentActionKey x, DeploymentActionKey y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id == y.Id;
            }
        
            public int GetHashCode(DeploymentActionKey obj)
            {
                unchecked
                {
                    var hashCode = (obj.Id != null ? obj.Id.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }
        
        public static IEqualityComparer<DeploymentActionKey> EqualityComparer { get; } = new DeploymentKeyEqualityComparer();

        // ReSharper disable once MemberCanBePrivate.Global
        public Guid Id { get; }

        [JsonConstructor]
        public DeploymentActionKey(Guid id)
        {
            Id = id;
        }

        public static implicit operator string(DeploymentActionKey deploymentActionKey)
        {
            return $"{deploymentActionKey.Id}";
        }
        
        public static implicit operator DeploymentActionKey(string raw)
        {
            return new DeploymentActionKey(Guid.Parse(raw));
        }
    }
    // public class DeploymentActionKey
    // {
    //     private sealed class DeploymentKeyEqualityComparer : IEqualityComparer<DeploymentActionKey>
    //     {
    //         public bool Equals(DeploymentActionKey x, DeploymentActionKey y)
    //         {
    //             if (ReferenceEquals(x, y)) return true;
    //             if (ReferenceEquals(x, null)) return false;
    //             if (ReferenceEquals(y, null)) return false;
    //             if (x.GetType() != y.GetType()) return false;
    //             return x.Application == y.Application &&
    //                    x.Environment == y.Environment &&
    //                    x.ImageRepository == y.ImageRepository && 
    //                    x.TagPropertyPath == y.TagPropertyPath && 
    //                    x.TargetTag == y.TargetTag;
    //         }
    //
    //         public int GetHashCode(DeploymentActionKey obj)
    //         {
    //             unchecked
    //             {
    //                 var hashCode = (obj.Application != null ? obj.Application.GetHashCode() : 0);
    //                 hashCode = (hashCode * 397) ^ (obj.Environment != null ? obj.Environment.GetHashCode() : 0);
    //                 hashCode = (hashCode * 397) ^ (obj.ImageRepository != null ? obj.ImageRepository.GetHashCode() : 0);
    //                 hashCode = (hashCode * 397) ^ (obj.TagPropertyPath != null ? obj.TagPropertyPath.GetHashCode() : 0);
    //                 hashCode = (hashCode * 397) ^ (obj.TargetTag != null ? obj.TargetTag.GetHashCode() : 0);
    //                 return hashCode;
    //             }
    //         }
    //     }
    //     
    //     public static IEqualityComparer<DeploymentActionKey> EqualityComparer { get; } = new DeploymentKeyEqualityComparer();
    //
    //     public DeploymentActionKey(ApplicationEnvironmentKey applicationEnvironmentKey, ApplicationEnvironmentImageSettings image, string targetTag, string currentTag)
    //         : this(
    //             applicationEnvironmentKey.Application, 
    //             applicationEnvironmentKey.Environment, 
    //             image.Repository,
    //             image.TagProperty.Path,
    //             targetTag, currentTag)
    //     {
    //
    //     }
    //
    //     public DeploymentActionKey(string application, string environment, string imageRepository, string tagPropertyPath, string targetTag, string currentTag)
    //     {
    //         Application = application;
    //         Environment = environment;
    //         ImageRepository = imageRepository;
    //         TagPropertyPath = tagPropertyPath;
    //         TargetTag = targetTag;
    //         CurrentTag = currentTag;
    //     }
    //
    //     /// <summary>
    //     ///     The application that this deployment will update.
    //     /// </summary>
    //     public string Application { get; }
    //
    //     /// <summary>
    //     ///     The environment that this deployment targets.
    //     /// </summary>
    //     public string Environment { get; }
    //     
    //     /// <summary>
    //     ///    Location of repository where images are stored. 
    //     /// </summary>
    //     public string ImageRepository { get; }
    //     
    //     /// <summary>
    //     ///     Path within manifest file that will be updated by this deployment.
    //     /// </summary>
    //     public string TagPropertyPath { get; }
    //
    //     public string TargetTag { get; }
    //     
    //     public string CurrentTag { get; }
    //
    //     public static implicit operator string(DeploymentActionKey deploymentActionKey)
    //     {
    //         return $"{deploymentActionKey.Application}:{deploymentActionKey.Environment}:{deploymentActionKey.ImageRepository}:{deploymentActionKey.TagPropertyPath}:{deploymentActionKey.TargetTag}:{deploymentActionKey.CurrentTag}";
    //     }
    //
    //     public static implicit operator DeploymentActionKey(DeploymentAction deploymentAction)
    //     {
    //         return new DeploymentActionKey(
    //             deploymentAction.ApplicationEnvironmentKey.Application,
    //             deploymentAction.ApplicationEnvironmentKey.Environment, 
    //             deploymentAction.Image.Repository, 
    //             deploymentAction.Image.TagProperty.Path,
    //             deploymentAction.TargetTag,
    //             deploymentAction.CurrentTag
    //         );
    //     }
    //
    //     public static implicit operator DeploymentActionKey(string raw)
    //     {
    //         var parts = raw.Split(':');
    //         
    //         if (parts.Length != 6) 
    //             throw new InvalidCastException();
    //
    //         return new DeploymentActionKey(parts[0], parts[1], parts[2], parts[3], parts[4], parts[5]);
    //     }
    //     
    // }
}