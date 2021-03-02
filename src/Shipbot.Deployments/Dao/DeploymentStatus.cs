namespace Shipbot.Deployments.Dao
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

    public enum DeploymentType
    {
        Unknown,
        ImageUpdate,
        PreviewRelease
    }
}