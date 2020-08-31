namespace Shipbot.Models
{
    public abstract class ImageUpdatePolicy
    {
        public abstract bool IsMatch(string value);
        
        public abstract bool IsGreaterThen(string left, string right);
    }
}