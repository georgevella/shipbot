using System.Collections.Generic;

namespace Shipbot.Models
{
    public class HelmApplicationSource : ApplicationSource
    {
        public List<string> ValuesFiles { get; set; }
        
        public List<string> Secrets { get; set; }
    }
}