namespace Shipbot.Controller.DTOs
{
    public enum DeploymentStatusDto
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