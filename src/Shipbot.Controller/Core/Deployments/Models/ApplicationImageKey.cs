using System;
using System.Collections.Generic;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Deployments.Models
{
    public class ApplicationImageKey
    {
        private sealed class ApplicationImageEqualityComparer : IEqualityComparer<ApplicationImageKey>
        {
            public bool Equals(ApplicationImageKey x, ApplicationImageKey y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Application == y.Application && x.Environment == y.Environment && x.ImageRepository == y.ImageRepository && x.TagPropertyPath == y.TagPropertyPath;
            }

            public int GetHashCode(ApplicationImageKey obj)
            {
                unchecked 
                {
                    return ((obj.Application != null ? obj.Application.GetHashCode() : 0) * 397) ^
                           ((obj.Environment != null ? obj.Environment.GetHashCode() : 0) * 397) ^
                           ((obj.TagPropertyPath != null ? obj.TagPropertyPath.GetHashCode() : 0) * 397) ^
                           (obj.ImageRepository != null ? obj.ImageRepository.GetHashCode() : 0)
                        ;
                }
            }
        }

        public static IEqualityComparer<ApplicationImageKey> EqualityComparer { get; } = new ApplicationImageEqualityComparer();

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

        public ApplicationImageKey(ApplicationEnvironmentKey applicationEnvironmentKey, Image image)
            : this(applicationEnvironmentKey.Application, applicationEnvironmentKey.Environment, image.Repository, image.TagProperty.Path)
        {
            
        }
        public ApplicationImageKey(string application,
            string environment,
            string imageRepository, 
            string tagPropertyPath)
        {
            Application = application;
            Environment = environment;
            ImageRepository = imageRepository;
            TagPropertyPath = tagPropertyPath;
        }

        public ApplicationImageKey()
        {
            
        }
        
        public static implicit operator string(ApplicationImageKey applicationImageKey)
        {
            return $"{applicationImageKey.Application}:{applicationImageKey.Environment}:{applicationImageKey.ImageRepository}:{applicationImageKey.TagPropertyPath}";
        }
    }
}