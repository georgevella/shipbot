using System;
using Shipbot.Models;

namespace Shipbot.Controller.Core.ApplicationSources.Models
{
    public class ApplicationSourceRepository
    {
        public Uri Uri { get; set; }
        
        public string Ref { get; set; }
        
        public GitCredentials Credentials { get; set; }
    }
}