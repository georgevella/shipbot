namespace Shipbot.Controller.Core.Configuration.ApplicationSources
{
    public class ApplicationSourceSettings
    {
        public ApplicationSourceType Type { get; set; }
        
        public ApplicationSourceRepository Repository { get; set; }
        
        public string Path { get; set; }
        
        public HelmApplicationSourceSettings Helm { get; set; }
    }
}