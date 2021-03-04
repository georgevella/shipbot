namespace Shipbot.Controller.DTOs
{
    public enum DeploymentStatusDto
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