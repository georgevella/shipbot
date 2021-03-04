using System.Text.RegularExpressions;

namespace Shipbot.Applications.Models
{
    public class RegexImageUpdatePolicy : ImageUpdatePolicy
    {
        public RegexImageUpdatePolicy(string pattern)
        {
            Pattern = pattern;
        }

        public string Pattern { get; }

        public override bool IsMatch(string value) => Regex.IsMatch(value, Pattern);
    }
}