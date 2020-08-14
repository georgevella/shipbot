using System;

namespace Shipbot.Models
{
    public class ApplicationSourceRepository
    {
        public Uri Uri { get; set; }
        
        public string Ref { get; set; }
        
        public GitCredentials Credentials { get; set; }
    }
}