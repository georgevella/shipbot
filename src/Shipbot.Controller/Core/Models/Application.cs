using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace Shipbot.Controller.Core.Models
{
    public class Application
    {
        public string Name { get; }
        public ImmutableList<Image> Images { get; }

        public ApplicationSource Source { get; }
        
        public bool AutoDeploy { get; }
        
        public NotificationSettings Notifications { get; }

        public Application(
            string name, 
            ImmutableList<Image> images,
            ApplicationSource source, 
            bool autoDeploy,
            NotificationSettings notifications
            )
        {
            Name = name;
            Images = images;
            Source = source;
            AutoDeploy = autoDeploy;
            Notifications = notifications;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class NotificationSettings
    {
        public NotificationSettings(List<string> channels = null)
        {
            Channels = channels ?? new List<string>();
        }

        public NotificationSettings(string channel)
        {
            Channels = new List<string>()
            {
                channel
            };
        }

        public List<string> Channels { get; }
    }
}