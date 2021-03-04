namespace Shipbot.Controller.Core.Configuration.Git
{
    public class GitCredentialSettings
    {
        public string Name { get; set; }
        
        public CredentialType Type { get; set; }
        
        public SshCredentialsSettings Ssh { get; set; }
        
        public UsernamePasswordCredentialsSettings UsernamePassword { get; set; }
    }
}