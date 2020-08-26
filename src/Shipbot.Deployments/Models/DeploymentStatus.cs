namespace Shipbot.Deployments.Models
{
    public enum DeploymentStatus
    {
        Pending,
        Starting,
        UpdatingManifests,
        Synchronizing,
        Synchronized,
        Complete,
        Failed
    }
}