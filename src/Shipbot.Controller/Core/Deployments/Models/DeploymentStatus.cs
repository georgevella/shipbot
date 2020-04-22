namespace Shipbot.Controller.Core.Deployments.Models
{
    public enum DeploymentStatus
    {
        /// <summary>
        ///     The deployment has been created and it's in a waiting state.
        /// </summary>
        Created,
        /// <summary>
        ///     The deployment has been queued in the deployment queue.
        /// </summary>
        Queued,
        /// <summary>
        ///     The deployment has moved from the queue and will be applied to the deployment sources.
        /// </summary>
        Deploying,
        /// <summary>
        ///     The deployment is back to a waiting state.
        /// </summary>
        Waiting,
        
        /// <summary>
        ///     The deployment is completed.
        /// </summary>
        Completed,
        Failed
    }
}