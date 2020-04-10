using System.Collections.Generic;
using Newtonsoft.Json;
using Shipbot.Controller.Core.Deployments.Models;

namespace Shipbot.Controller.RestApiDto
{
    public class DeploymentDto
    {
        public string Id { get; }
        
        public List<DeploymentActionDto> Actions { get; } = new List<DeploymentActionDto>();

        public DeploymentDto(string id)
        {
            Id = id;
        }
    }
    
    
    public class DeploymentActionDto
    {
        public static implicit operator DeploymentActionDto(DeploymentAction deploymentAction)
        {
            return new DeploymentActionDto(
                deploymentAction.ApplicationEnvironmentKey.Environment,
                deploymentAction.Image.Repository,
                deploymentAction.CurrentTag,
                deploymentAction.TargetTag,
                deploymentAction.Status
            );
        }
        
        [JsonConstructor]
        public DeploymentActionDto(string environment, string image, string currentTag, string targetTag, DeploymentActionStatus status)
        {
            Environment = environment;
            Image = image;
            CurrentTag = currentTag;
            TargetTag = targetTag;
            Status = status;
        }

        public string Environment { get; }
        
        public string Image { get; }
        
        public string CurrentTag { get; }
        
        public string TargetTag { get; }

        public DeploymentActionStatus Status { get; }
    }
}