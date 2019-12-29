using System;

namespace Shipbot.Controller.Core.DeploymentSources.Models
{
    public class ApplicationSourceRepository
    {
        public Uri Uri { get; set; }
        
        public string Ref { get; set; }
        
        public string CredentialsKey { get; set; }
    }
}