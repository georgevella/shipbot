using System;
using Shipbot.Controller.Core.ApplicationSources;

namespace Shipbot.Controller.Core.Models
{
    public class ApplicationSourceRepository
    {
        public Uri Uri { get; set; }
        
        public string Ref { get; set; }
        
        public GitCredentials Credentials { get; set; }
    }
}