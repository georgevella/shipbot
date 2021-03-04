using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shipbot.Data;

namespace Shipbot.Deployments.Dao
{
    [Table("deploymentQueue")]
    public class DeploymentQueue
    {
        private Deployment _deployment;
        private Action<object, string> LazyLoader { get; }

        [Key]
        public Guid Id { get; set; }

        public Deployment Deployment
        {
            get => LazyLoader.Load(this, ref _deployment);
            set => _deployment = value;
        }

        public Guid DeploymentId { get; set; }
        
        /// <summary>
        ///     Id of application that this deployment will affect.
        /// </summary>
        public string ApplicationId { get; set; }
        
        /// <summary>
        ///     Date and Time when the deployment was added to the queue.
        /// </summary>
        public DateTimeOffset CreationDateTime { get; set; }

        /// <summary>
        ///     Date and Time when the deployment is available for processing.
        /// </summary>
        public DateTimeOffset AvailableDateTime { get; set; }
        
        /// <summary>
        ///     Date and Time when the item was dequeued and processed successfully.
        /// </summary>
        public DateTimeOffset? AcknowledgeDateTime { get; set; }
        
        /// <summary>
        ///     Number of times the deployment executor (deployment sources) tried to apply this deployment.
        /// </summary>
        public int AttemptCount { get; set; }

        public DeploymentQueue(Action<object, string> lazyLoader)
        {
            LazyLoader = lazyLoader;
        }

        public DeploymentQueue()
        {
            
        }
    }
}