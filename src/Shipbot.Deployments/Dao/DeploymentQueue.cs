using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shipbot.Deployments.Dao
{
    [Table("deploymentQueue")]
    public class DeploymentQueue
    {
        [Key]
        public Guid Id { get; set; }
        
        public Deployment Deployment { get; set; }
        
        public DateTime QueueDateTime { get; set; }
        
        public string ApplicationId { get; set; }
    }
}