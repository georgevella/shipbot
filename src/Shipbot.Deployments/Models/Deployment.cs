using System;

namespace Shipbot.Deployments.Models
{
    public class Deployment
    {
        public Guid Id { get; }
        public string ApplicationId { get; }
        public string ImageRepository { get; }
        public string UpdatePath { get; }
        public string CurrentTag { get; }
        public string TargetTag { get; }
        public DeploymentStatus Status { get; }

        public Deployment(
            Guid id,
            string applicationId,
            string imageRepository, 
            string updatePath, 
            string currentTag, 
            string targetTag, 
            DeploymentStatus status
            )
        {
            Id = id;
            ApplicationId = applicationId;
            ImageRepository = imageRepository;
            UpdatePath = updatePath;
            CurrentTag = currentTag;
            TargetTag = targetTag;
            Status = status;
        }
    }
    
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