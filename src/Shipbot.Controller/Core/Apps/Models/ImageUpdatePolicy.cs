namespace Shipbot.Controller.Core.Apps.Models
{
    public abstract class ImageUpdatePolicy
    {
        public abstract bool IsMatch(string value);

        public abstract bool IsGreaterThen(string tag, string currentTag);
    }
}