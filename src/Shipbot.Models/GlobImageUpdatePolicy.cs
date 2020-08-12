using DotNet.Globbing;

namespace Shipbot.Controller.Core.Models
{
    public class GlobImageUpdatePolicy : ImageUpdatePolicy
    {
        private readonly Glob _pattern;

        public GlobImageUpdatePolicy(string pattern)
        {
            _pattern = Glob.Parse(pattern);
        }

        public override bool IsMatch(string value)
        {
            return _pattern.IsMatch(value);
        }
    }
}