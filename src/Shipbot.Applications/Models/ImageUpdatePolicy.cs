namespace Shipbot.Applications.Models
{
    public abstract class ImageUpdatePolicy
    {
        public static ImageUpdatePolicy NoOp { get; } = new NoOpImageUpdatePolicy();
        
        public abstract bool IsMatch(string value);
    }
}