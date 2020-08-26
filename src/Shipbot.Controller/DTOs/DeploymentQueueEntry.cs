using System;

namespace Shipbot.Controller.DTOs
{
    public class DeploymentQueueEntry
    {
        public Guid DeploymentId { get; set; }
        
        public int? Delay { get; set; }
    }
}