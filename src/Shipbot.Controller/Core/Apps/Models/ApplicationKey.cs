namespace Shipbot.Controller.Core.Apps.Models
{
    public class ApplicationKey
    {
        public string Name { get; set; }

        public ApplicationKey(string name)
        {
            Name = name;
        }

        public static implicit operator string(ApplicationKey applicationKey)
        {
            return applicationKey.Name;
        }

        public static implicit operator ApplicationKey(string name)
        {
            return new ApplicationKey(name);    
        }
    }
}