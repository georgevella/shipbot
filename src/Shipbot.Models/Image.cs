using System.Linq;

namespace Shipbot.Models
{
    public class Image
    {
        public Image(string repository, TagProperty tagProperty, ImageUpdatePolicy policy)
        {
            Repository = repository;
            TagProperty = tagProperty;
            Policy = policy;
        }

        public override string ToString()
        {
            return this.Repository;
        }

        protected bool Equals(Image other)
        {
            return string.Equals(Repository, other.Repository) && Equals(TagProperty, other.TagProperty) && Equals(Policy, other.Policy);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Image) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Repository != null ? Repository.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TagProperty != null ? TagProperty.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Policy != null ? Policy.GetHashCode() : 0);
                return hashCode;
            }
        }

        public string Repository { get; }

        public TagProperty TagProperty { get; }

        public ImageUpdatePolicy Policy { get; }

        public string ShortRepository => Repository.Any() ? Repository.Substring(Repository.IndexOf('/') + 1) : string.Empty; 
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

        public string Path { get; }
        
        public TagPropertyValueFormat ValueFormat { get; }
    }
    
    public enum TagPropertyValueFormat
    {
        TagOnly,
        TagAndRepository
    }
}
