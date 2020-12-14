namespace Shipbot.Deployments.Dao
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