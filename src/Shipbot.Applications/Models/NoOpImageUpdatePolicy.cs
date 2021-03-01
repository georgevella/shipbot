namespace Shipbot.Applications.Models
{
    public class NoOpImageUpdatePolicy : ImageUpdatePolicy
    {
        public override bool IsMatch(string value) => false;
    }
}