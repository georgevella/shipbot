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

        public string Pattern => _pattern.ToString();

        public override bool IsMatch(string value)
        {
            return _pattern.IsMatch(value);
        }
    }
}