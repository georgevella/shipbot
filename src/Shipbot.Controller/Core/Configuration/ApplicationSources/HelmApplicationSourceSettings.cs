using System.Collections.Generic;

namespace Shipbot.Controller.Core.Configuration.ApplicationSources
{
    public class HelmApplicationSourceSettings
    {
        public List<string> ValueFiles { get; set; }
        
        public List<string> Secrets { get; set; }
    }
}