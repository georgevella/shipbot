using System.Collections.Generic;
using Shipbot.Controller.Core.Apps.Models;

namespace Shipbot.Controller.Core.Apps.GrainState
{
    public class ApplicationIndex
    {
        public HashSet<ApplicationKey> Applications { get; } = new HashSet<ApplicationKey>();
    }
}