namespace Shipbot.Controller.Core.Git.Models
{
    public abstract class GitCredentials
    {
        
    }

    public class UsernamePasswordGitCredentials : GitCredentials
    {
        public UsernamePasswordGitCredentials(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public UsernamePasswordGitCredentials()
        {
            
        }

        public string Username { get; set; }
        
        public string Password { get; set;  }
    }

    public class SshGitCredentials : GitCredentials
    {
        public string SshPrivateKey { get; set; }

        public SshGitCredentials()
        {
            
        }
        
        public SshGitCredentials(string sshPrivateKey)
        {
            SshPrivateKey = sshPrivateKey;
        }
    }
}