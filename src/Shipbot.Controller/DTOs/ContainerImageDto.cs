using System;
using Shipbot.ContainerRegistry.Models;

namespace Shipbot.Controller.DTOs
{
    public class ContainerImageDto
    {    
        public string Repository { get; set; }
        
        public string Hash { get; set; }
        
        public string Tag { get; set; }
        
        public DateTimeOffset CreationDateTime { get; set; }

        public ContainerImageDto()
        {
            
        }

        public static implicit operator ContainerImageDto(ContainerImage containerImage)
        {
            if (containerImage == null) throw new ArgumentNullException(nameof(containerImage));
            return new ContainerImageDto()
            {
                Hash = containerImage.Hash,
                Repository = containerImage.Repository,
                Tag = containerImage.Tag,
                CreationDateTime = containerImage.CreationDateTime
            };
        }
    }
}