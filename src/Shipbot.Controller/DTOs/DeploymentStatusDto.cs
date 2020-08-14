namespace Shipbot.Controller.DTOs
{
    public enum DeploymentStatusDto
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