namespace Shipbot.Deployments.Models
{
    public enum DeploymentStatus
    {
        Unknown,
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