using System;
using Newtonsoft.Json;

namespace Shipbot.Controller.DTOs
{
    public class DeploymentQueueEntry
    {
        public Guid DeploymentId { get; }
        public int? Delay { get; }
        
        public bool Force { get; }

        [JsonConstructor]
        public DeploymentQueueEntry(Guid deploymentId, int? delay, bool force)
        {
            DeploymentId = deploymentId;
            Delay = delay;
            Force = force;
        }
    }
}