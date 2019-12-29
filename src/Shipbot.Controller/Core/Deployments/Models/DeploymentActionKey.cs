using System;
using System.Collections.Generic;
using Microsoft.Extensions.ObjectPool;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Deployments.Models
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
                return x.Application == y.Application &&
                       x.Environment == y.Environment &&
                       x.ImageRepository == y.ImageRepository && 
                       x.TagPropertyPath == y.TagPropertyPath && 
                       x.TargetTag == y.TargetTag;
            }

            public int GetHashCode(DeploymentActionKey obj)
            {
                unchecked
                {
                    var hashCode = (obj.Application != null ? obj.Application.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.Environment != null ? obj.Environment.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.ImageRepository != null ? obj.ImageRepository.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.TagPropertyPath != null ? obj.TagPropertyPath.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.TargetTag != null ? obj.TargetTag.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }
        
        public static IEqualityComparer<DeploymentActionKey> EqualityComparer { get; } = new DeploymentKeyEqualityComparer();

        public DeploymentActionKey(ApplicationEnvironmentKey applicationEnvironmentKey, Image image, string targetTag)
            : this(
                applicationEnvironmentKey.Application, 
                applicationEnvironmentKey.Environment, 
                image.Repository,
                image.TagProperty.Path,
                targetTag)
        {

        }

        public DeploymentActionKey(string application, string environment, string imageRepository, string tagPropertyPath, string targetTag)
        {
            Application = application;
            Environment = environment;
            ImageRepository = imageRepository;
            TagPropertyPath = tagPropertyPath;
            TargetTag = targetTag;
        }

        public DeploymentActionKey()
        {
            
        }

        /// <summary>
        ///     The application that this deployment will update.
        /// </summary>
        public string Application { get; set; }

        /// <summary>
        ///     The environment that this deployment targets.
        /// </summary>
        public string Environment { get; set; }
        
        /// <summary>
        ///    Location of repository where images are stored. 
        /// </summary>
        public string ImageRepository { get; set; }
        
        /// <summary>
        ///     Path within manifest file that will be updated by this deployment.
        /// </summary>
        public string TagPropertyPath { get; set; }

        public string TargetTag { get; set; }

        public static implicit operator string(DeploymentActionKey deploymentActionKey)
        {
            return $"{deploymentActionKey.Application}:{deploymentActionKey.Environment}:{deploymentActionKey.ImageRepository}:{deploymentActionKey.TagPropertyPath}:{deploymentActionKey.TargetTag}";
        }

        public static implicit operator DeploymentActionKey(string raw)
        {
            var parts = raw.Split(':');
            
            if (parts.Length != 5) 
                throw new InvalidCastException();

            return new DeploymentActionKey(parts[0], parts[1], parts[2], parts[3], parts[4]);
        }
        
    }
}