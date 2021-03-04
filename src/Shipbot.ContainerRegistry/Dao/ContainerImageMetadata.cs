using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shipbot.ContainerRegistry.Dao
{
    [Table("containerImageMetadata")]
    public class ContainerImageMetadata
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid RepositoryId { get; set; }
        public ContainerImageRepository Repository { get; set; }
        
        public string Hash { get; set; }
        
        public DateTime CreatedDateTime { get; set; }
    }
}