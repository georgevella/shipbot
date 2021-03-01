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

    public enum DeploymentType
    {
        ImageUpdate,
        PreviewRelease
    }
}