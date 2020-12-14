using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shipbot.Deployments.Dao
{
    [Table("deployments")]
    public class Deployment
    {
        [Key]
        public Guid Id { get; set; }
        
        public DateTime CreationDateTime { get; set; }
        
        public DateTime? DeploymentDateTime { get; set; }
        
        /// <summary>
        ///     Id of application that this deployment will target.
        /// </summary>
        public string ApplicationId { get; set; }
        
        /// <summary>
        ///     Repository of image to deploy.
        /// </summary>
        public string ImageRepository { get; set; }
        
        /// <summary>
        ///     Path in yaml file that will be updated with the new image tag.
        /// </summary>
        public string UpdatePath { get; set; }
        
        /// <summary>
        ///     Current tag of deployed image.
        /// </summary>
        public string CurrentImageTag { get; set; }
        
        /// <summary>
        ///     Tag of image to be deployed.
        /// </summary>
        public string TargetImageTag { get; set; }
        
        public bool IsAutomaticDeployment { get; set; }
        
        public DeploymentStatus Status { get; set; }
    }
}