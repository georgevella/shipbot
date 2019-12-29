using System.Collections.Generic;

namespace Shipbot.Controller.Core.Apps.Models
{
    public class NotificationSettings
    {
        public List<string> Channels { get; set; } = new List<string>();
    }
}