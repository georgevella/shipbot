using System.Collections.Generic;
using System.Linq;

namespace Shipbot.Controller.Core.ApplicationSources.Models
{
    public class HelmApplicationSource : ApplicationSource
    {
        public IEnumerable<string> ValuesFiles { get; }
        
        public IEnumerable<string> Secrets { get; }

        public HelmApplicationSource(
            string application, 
            ApplicationSourceRepository repository, 
            string path,
            IEnumerable<string> valueFiles,
            IEnumerable<string> secretFiles
            ) : base(application, repository, path)
        {
            ValuesFiles = valueFiles?.ToList() ?? Enumerable.Empty<string>();
            Secrets = secretFiles?.ToList() ?? Enumerable.Empty<string>();
        }
    }
}