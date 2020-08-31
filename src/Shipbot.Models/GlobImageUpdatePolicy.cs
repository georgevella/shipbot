using System.Text;
using System.Text.RegularExpressions;
using DotNet.Globbing;

namespace Shipbot.Models
{
    public class GlobImageUpdatePolicy : ImageUpdatePolicy
    {
        private readonly Glob _pattern;
        private readonly string _regexPattern;

        public GlobImageUpdatePolicy(string pattern)
        {
            _pattern = Glob.Parse(pattern);
            
            _regexPattern = Regex.Escape( pattern )
                .Replace( @"\*", "(.*)" )
                .Replace( @"\?", "(.)" )
                ;
        }

        public override bool IsMatch(string value)
        {
            return _pattern.IsMatch(value);
        }

        public override bool IsGreaterThen(string left, string right)
        {
            var leftResult = Regex.Match(left, _regexPattern);
            var rightResult = Regex.Match(right, _regexPattern);
            
            
            return left?.CompareTo(right) > 0;
        }
    }
}