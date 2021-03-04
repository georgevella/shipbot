using System;
using System.Collections.Generic;

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
        
        public DeploymentType Type { get; }
        
        public string NameSuffix { get; }
        
        public string InstanceId { get; }
        
        public IReadOnlyDictionary<string, string> Parameters { get; } 
        
        public DateTime CreationDateTime { get; }
        
        public DateTime? DeploymentDateTime { get; }

        public Deployment(
            Guid id,
            string applicationId,
            string imageRepository, 
            string updatePath, 
            string currentTag, 
            string targetTag, 
            DeploymentStatus status, 
            DeploymentType type, 
            string nameSuffix, 
            DateTime creationDateTime,
            DateTime? deploymentDateTime = null,
            string instanceId = "", 
            IReadOnlyDictionary<string, string>? parameters = null
            )
        {
            Id = id;
            ApplicationId = applicationId;
            ImageRepository = imageRepository;
            UpdatePath = updatePath;
            CurrentTag = currentTag;
            TargetTag = targetTag;
            Status = status;
            Type = type;
            NameSuffix = nameSuffix;
            InstanceId = instanceId;
            Parameters = parameters ?? new Dictionary<string, string>();
        }
    }
}