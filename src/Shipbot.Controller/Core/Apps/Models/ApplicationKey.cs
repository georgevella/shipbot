namespace Shipbot.Controller.Core.Apps.Models
{
    public class ApplicationKey
    {
        protected bool Equals(ApplicationKey other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ApplicationKey)) return false;
            return Equals((ApplicationKey) obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public string Name { get; set; }

        public ApplicationKey(string name)
        {
            Name = name;
        }

        public static implicit operator string(ApplicationKey applicationKey)
        {
            return applicationKey.Name;
        }

        public static implicit operator ApplicationKey(string name)
        {
            return new ApplicationKey(name);    
        }
    }
}