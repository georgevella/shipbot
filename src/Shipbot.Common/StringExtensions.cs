using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Shipbot.Common
{
    public static class StringExtensions
    {
        public static IReadOnlyDictionary<string, string> ExtractParametersWithRegex(this string s, string pattern)
        {
            var match = Regex.Match(s, pattern);
            return match.Success 
                ? match.Groups.ToDictionary( x=>x.Name, x=>x.Value) 
                : new Dictionary<string, string>();
        }
    }
}
