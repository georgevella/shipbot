using DotNet.Globbing;

namespace Shipbot.Controller.Core.Apps.Models
{
    public class GlobImageUpdatePolicy : ImageUpdatePolicy
    {
        protected bool Equals(GlobImageUpdatePolicy other)
        {
            return Pattern == other.Pattern;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GlobImageUpdatePolicy) obj);
        }

        public override int GetHashCode()
        {
            return Pattern.GetHashCode();
        }

        public string Pattern { get; set; }

        public GlobImageUpdatePolicy()
        {
            
        }
        
        public GlobImageUpdatePolicy(string pattern)
        {
            Pattern = pattern;
        }

        public override bool IsMatch(string value)
        {
            return Glob.Parse(Pattern).IsMatch(value);
        }

        public override bool IsGreaterThen(string tag, string currentTag)
        {
            return tag?.CompareTo(currentTag) < 0;
        }
    }
}