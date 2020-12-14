namespace Shipbot.Models
{
    public class RegexImageUpdatePolicy : ImageUpdatePolicy
    {
        public RegexImageUpdatePolicy(string pattern)
        {
            Pattern = pattern;
            throw new System.NotImplementedException();
        }

        public string Pattern { get; }

        public override bool IsMatch(string value)
        {
            throw new System.NotImplementedException();
        }
    }
}