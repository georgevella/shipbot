using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shipbot.ContainerRegistry.Dao
{
    [Table("containerImageTags")]
    public class ContainerImageTag
    {
        public Guid Id { get; set; }
        
        public Guid RepositoryId { get; set; }
        public ContainerImageRepository Repository { get; set; }
        
        public ContainerImageMetadata Metadata { get; set; }
        
        public string Tag { get; set; }
    }
}