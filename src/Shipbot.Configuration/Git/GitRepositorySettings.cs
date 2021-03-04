namespace Shipbot.Controller.Core.Configuration.Git
{
    public class GitRepositorySettings
    {
        public string Uri { get; set; }
        
        public string Ref { get; set; }
        
        public string Credentials { get; set; }
    }
}