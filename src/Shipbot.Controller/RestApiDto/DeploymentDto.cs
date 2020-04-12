using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Shipbot.Controller.Core.Deployments.Models;
using JsonException = System.Text.Json.JsonException;

namespace Shipbot.Controller.RestApiDto
{
    public class NewDeploymentDtoJsonConverter : JsonConverter<NewDeploymentDto>
    {
        public override NewDeploymentDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            };

            string? repository = null;
            string? tag = null;

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                reader.Read();

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    if (reader.ValueTextEquals("Repository"))
                    {
                        reader.Read();
                        repository = reader.GetString();
                    }

                    if (reader.ValueTextEquals("Tag"))
                    {
                        reader.Read();
                        tag = reader.GetString();
                    }
                }
            }
            
            if (!string.IsNullOrEmpty(repository) && !string.IsNullOrEmpty(tag))
                return new NewDeploymentDto(repository, tag);
            
            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, NewDeploymentDto value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    [JsonConverter(typeof(NewDeploymentDtoJsonConverter))]
    public class NewDeploymentDto
    {
        public NewDeploymentDto(string repository, string tag)
        {
            Repository = repository;
            Tag = tag;
        }

        public string Repository { get; }
        
        public string Tag { get; }
    }
    
    public class DeploymentDto : NewDeploymentDto
    {
        public string Id { get; }
        
        public DeploymentStatus Status { get; }

        public List<DeploymentActionDto> Actions { get; } = new List<DeploymentActionDto>();

        public DeploymentDto(string id, string repository, string tag, DeploymentStatus status) : base(repository, tag)
        {
            Id = id;
            Status = status;
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