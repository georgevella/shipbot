using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shipbot.SlackIntegration.Dao
{
    /// <summary>
    ///     A DAO entity that describes the message sent in relation to a deployment.
    /// </summary>
    [Table("deploymentNotifications")]
    public class DeploymentNotification
    {
        [Key]
        public Guid Id { get; set; }
        
        public SlackMessage SlackMessage { get; set; }

        public Guid DeploymentId { get; set; }
    }
}