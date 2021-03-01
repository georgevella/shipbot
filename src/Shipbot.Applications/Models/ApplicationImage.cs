using System;
using System.Linq;
using Shipbot.Controller.Core.Configuration.Apps;
using Shipbot.Models;

namespace Shipbot.Applications.Models
{
    public class ApplicationImage
    {
        public ApplicationImage(
            string repository, 
            TagProperty tagProperty, 
            ImageUpdatePolicy policy,
            DeploymentSettings deploymentSettings,
            ApplicationImageSourceCode sourceCode, 
            ApplicationImageIngress ingress)
        {
            Repository = repository;
            TagProperty = tagProperty;
            Policy = policy;
            DeploymentSettings = deploymentSettings;
            SourceCode = sourceCode;
            Ingress = ingress;
        }

        public override string ToString()
        {
            return this.Repository;
        }

        protected bool Equals(ApplicationImage other)
        {
            return string.Equals(Repository, other.Repository) && Equals(TagProperty, other.TagProperty) && Equals(Policy, other.Policy);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationImage) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Repository != null ? Repository.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TagProperty != null ? TagProperty.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Policy != null ? Policy.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DeploymentSettings != null ? DeploymentSettings.GetHashCode() : 0);
                return hashCode;
            }
        }

        public string Repository { get; }
        
        public TagProperty TagProperty { get; }

        public ImageUpdatePolicy Policy { get; }

        public DeploymentSettings DeploymentSettings { get; }
        
        public ApplicationImageIngress Ingress { get; }
        
        public ApplicationImageSourceCode SourceCode { get; }

        public string ShortRepository => Repository.Any() ? Repository.Substring(Repository.IndexOf('/') + 1) : string.Empty; 
    }

    public class ApplicationImageIngress
    {
        public static readonly ApplicationImageIngress Empty = new ApplicationImageIngress();
        
        private ApplicationImageIngress(bool isAvailable, string domain)
        {
            if (domain == null)
                throw new ArgumentNullException(nameof(domain));
            
            if (isAvailable && string.IsNullOrWhiteSpace(domain))
                throw new ArgumentOutOfRangeException(nameof(domain));
            
            IsAvailable = isAvailable;
            Domain = domain;
        }

        public bool IsAvailable { get; }
        
        public string Domain { get; }

        private ApplicationImageIngress() : this(false, string.Empty)
        {
            
        }
        
        public ApplicationImageIngress(string domain): this(true, domain)
        {
            
        }
    }

    public class ApplicationImageSourceCode
    {
        public readonly static ApplicationImageSourceCode Empty = new ApplicationImageSourceCode();

        private ApplicationImageSourceCode()
        {
            Enabled = false;
            Github = GithubApplicationSourceCodeRepository.Empty;
            RepositoryType = ApplicationSourceCodeRepositoryType.None;
        }
        public ApplicationImageSourceCode(bool enabled, GithubApplicationSourceCodeRepository github)
        {
            Enabled = enabled;
            RepositoryType = ApplicationSourceCodeRepositoryType.Github;
            Github = github;
        }

        public bool Enabled { get; }
        
        public ApplicationSourceCodeRepositoryType RepositoryType { get; }
        
        public GithubApplicationSourceCodeRepository Github { get; }

        public bool IsAvailable => Enabled && RepositoryType != ApplicationSourceCodeRepositoryType.None;
    }

    public class GithubApplicationSourceCodeRepository
    {
        public static GithubApplicationSourceCodeRepository Empty =
            new GithubApplicationSourceCodeRepository();

        private GithubApplicationSourceCodeRepository()
        {
            Owner = string.Empty;
            Repository = string.Empty;
        }
        
        public GithubApplicationSourceCodeRepository(string owner, string repository)
        {
            Owner = !string.IsNullOrWhiteSpace(owner) ? owner : throw new ArgumentOutOfRangeException(nameof(owner));
            Repository = !string.IsNullOrWhiteSpace(repository) ? repository : throw new ArgumentOutOfRangeException(nameof(repository));
        }

        public string Owner { get; }
        
        public string Repository { get; }

        public bool IsValid => (Owner.Length > 0 && Repository.Length > 0);
    }

    public enum ApplicationSourceCodeRepositoryType
    {
        None,
        Github
    }

    public class DeploymentSettings
    {
        protected bool Equals(DeploymentSettings other)
        {
            return AutomaticallyCreateDeploymentOnImageRepositoryUpdate == other.AutomaticallyCreateDeploymentOnImageRepositoryUpdate && AutomaticallySubmitDeploymentToQueue == other.AutomaticallySubmitDeploymentToQueue;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DeploymentSettings) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AutomaticallyCreateDeploymentOnImageRepositoryUpdate, AutomaticallySubmitDeploymentToQueue);
        }

        public bool AutomaticallyCreateDeploymentOnImageRepositoryUpdate { get; }
        
        public bool AutomaticallySubmitDeploymentToQueue { get; }
        
        public PreviewReleaseDeploymentSettings PreviewReleases { get; }

        public DeploymentSettings(
            bool automaticallyCreateDeploymentOnImageRepositoryUpdate, 
            bool automaticallySubmitDeploymentToQueue,
            PreviewReleaseDeploymentSettings? previewReleases = null)
        {
            AutomaticallyCreateDeploymentOnImageRepositoryUpdate = automaticallyCreateDeploymentOnImageRepositoryUpdate;
            AutomaticallySubmitDeploymentToQueue = automaticallySubmitDeploymentToQueue;
            PreviewReleases = previewReleases ?? 
                              new PreviewReleaseDeploymentSettings(
                                   false, 
                                  ImageUpdatePolicy.NoOp, 
                                  string.Empty);
        }
    }

    public class PreviewReleaseDeploymentSettings
    {
        public PreviewReleaseDeploymentSettings(bool enabled, ImageUpdatePolicy policy, string tagPatternRegex)
        {
            Enabled = enabled;
            Policy = policy;
            TagPatternRegex = tagPatternRegex;
        }

        /// <summary>
        ///     Set to true if preview release management is enabled or not.
        /// </summary>
        public bool Enabled { get; }
        
        /// <summary>
        ///     Policy used to detect which docker images are preview release images
        /// </summary>
        public ImageUpdatePolicy Policy { get; }
        
        /// <summary>
        ///     Regex that extracts parameters from the image tag.
        /// </summary>
        public string TagPatternRegex { get; }
    }

    public class TagProperty
    {
        public TagProperty(string path, TagPropertyValueFormat valueFormat)
        {
            Path = path;
            ValueFormat = valueFormat;
        }

        protected bool Equals(TagProperty other)
        {
            return string.Equals(Path, other.Path) && ValueFormat == other.ValueFormat;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TagProperty) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Path != null ? Path.GetHashCode() : 0) * 397) ^ (int) ValueFormat;
            }
        }

        /// <summary>
        ///     JSON/YAML Path in deployment sources where the tag of a deployed service / container
        ///     can be read from or written to.  
        /// </summary>
        public string Path { get; }
        
        /// <summary>
        ///     The contents of the value read from / written to the path in the deployment sources.
        /// </summary>
        public TagPropertyValueFormat ValueFormat { get; }
    }
    
    public enum TagPropertyValueFormat
    {
        TagOnly,
        TagAndRepository
    }
}
