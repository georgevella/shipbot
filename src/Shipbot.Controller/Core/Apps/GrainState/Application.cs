using System.Collections.Generic;
using Shipbot.Controller.Core.Apps.Models;

namespace Shipbot.Controller.Core.Apps.GrainState
{
    public class Application
    {
        public HashSet<ApplicationEnvironmentKey> EnvironmentKeys { get; } 
            = new HashSet<ApplicationEnvironmentKey>(ApplicationEnvironmentKey.EqualityComparer);
        public NotificationSettings Notifications { get; set; }
    }
}