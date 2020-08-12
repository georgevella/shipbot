namespace Shipbot.Controller.Core.Models
{
    public abstract class ApplicationSource
    {
        public ApplicationSourceRepository Repository { get; set; }
        
        public string Path { get; set; }
    }
}