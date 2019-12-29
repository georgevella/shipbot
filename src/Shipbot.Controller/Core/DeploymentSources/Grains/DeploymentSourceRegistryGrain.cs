using System.Collections.Generic;
using Orleans;
using Shipbot.Controller.Core.DeploymentSources.Models;

namespace Shipbot.Controller.Core.DeploymentSources.Grains
{
    public class DeploymentSourceRegistryGrain : Grain<Dictionary<DeploymentSourceKey, string>>
    {
        
    }
}