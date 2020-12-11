using System;

namespace Shipbot.ContainerRegistry.Models
{
    public class ContainerImage
    {
        public static ContainerImage Empty { get; } =
            new ContainerImage(string.Empty, string.Empty, string.Empty, DateTimeOffset.MinValue);
        
        protected bool Equals(ContainerImage other)
        {
            return Repository == other.Repository && Hash == other.Hash && Tag == other.Tag && CreationDateTime.Equals(other.CreationDateTime);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ContainerImage) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Repository, Hash, Tag, CreationDateTime);
        }

        public string Repository { get; }
        
        public string Hash { get; }
        
        public string Tag { get; }
        
        public DateTimeOffset CreationDateTime { get; }

        public ContainerImage(string repository, string tag) : this(repository, tag, $"{repository}:{tag}".GetHashCode().ToString(), DateTimeOffset.Now) { }
        
        public ContainerImage(string repository, string tag, DateTimeOffset creationDateTime) : this(repository, tag, $"{repository}:{tag}@{creationDateTime:O}".GetHashCode().ToString(), creationDateTime) { }
        public ContainerImage(string repository, string tag, string hash) : this(repository, tag, hash, DateTimeOffset.Now) { }
        
        public ContainerImage(string repository, string tag, string hash, DateTimeOffset creationDateTime)
        {
            Repository = repository;
            Hash = hash;
            Tag = tag;
            CreationDateTime = creationDateTime;
        }
    }
}