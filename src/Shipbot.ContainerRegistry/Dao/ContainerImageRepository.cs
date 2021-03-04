using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shipbot.ContainerRegistry.Dao
{
    [Table("containerImageRepositories")]
    public class ContainerImageRepository
    {
        [Key]
        public Guid Id { get; set; }
        
        public string Name { get; set; }
    }
}