using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Shipbot.Controller.Core.Deployments.GrainKeys;

namespace Shipbot.Controller.Core.Deployments.Models
{
    public class InternalDeploymentQueueItem
    {
        [JsonConstructor]
        public InternalDeploymentQueueItem(DateTime queuedOn, DeploymentActionKey action)
        {
            QueuedOn = queuedOn;
            Action = action;
        }

        public DeploymentActionKey Action { get; }
        
        public DateTime QueuedOn { get; }
    }
    
    public class DeploymentQueueItem
    {
        [JsonConstructor]
        public DeploymentQueueItem(DateTime queuedOn,
            DeploymentActionKey action,
            string application,
            string environment,
            string currentTag,
            string targetTag,
            DeploymentActionStatus status)
        {
            QueuedOn = queuedOn;
            Action = action;
            Application = application;
            Environment = environment;
            CurrentTag = currentTag;
            TargetTag = targetTag;
            Status = status;
        }

        public DeploymentActionStatus Status { get; }

        public DateTime QueuedOn { get; }
        public DeploymentActionKey Action { get; }
        public string Application { get; }
        public string Environment { get; }
        public string CurrentTag { get; }
        public string TargetTag { get; }
    }
}