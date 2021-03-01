namespace Shipbot.Applications.Models
{
    public class ApplicationImageInstance
    {
        public ApplicationImageInstance(ApplicationImageInstanceType type, string name, string currentTag)
        {
            Type = type;
            Name = name;
            CurrentTag = currentTag;
        }

        public ApplicationImageInstanceType Type { get; }
        
        public string CurrentTag { get; }
        
        public string Name { get; }
    }

    public enum ApplicationImageInstanceType
    {
        Primary,
        Preview
    }
}