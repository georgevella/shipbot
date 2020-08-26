namespace Shipbot.Deployments.Models
{
    public enum DeploymentStatus
    {
        Pending,
        Queued,
        Starting,
        UpdatingManifests,
        Synchronizing,
        Synchronized,
        Complete,
        Failed
    }
}