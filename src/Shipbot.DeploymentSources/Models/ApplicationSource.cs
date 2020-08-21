namespace Shipbot.Controller.Core.ApplicationSources.Models
{
    public abstract class ApplicationSource
    {
        protected ApplicationSource(string application, ApplicationSourceRepository repository, string path)
        {
            Application = application;
            Repository = repository;
            Path = path;
        }

        public string Application { get; }
        public ApplicationSourceRepository Repository { get;  }
        
        public string Path { get; }
    }
}