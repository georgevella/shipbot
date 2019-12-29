using System.Collections.Generic;
using Shipbot.Controller.Core.ContainerRegistry.Models;

namespace Shipbot.Controller.Core.ContainerRegistry.GrainState
{
    public class RegistryWatcherGrainState
    {
        public ImageTagCollection Tags { get; } 
        
        public RegistryWatcherGrainState()
        {
            Tags = new ImageTagCollection();
        }
    }
}