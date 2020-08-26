using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shipbot.SlackIntegration.Dao
{
    [Table("slackMessages")]
    public class SlackMessage
    {
        [Key]
        public Guid Id { get; set; }
        
        public string Timestamp { get; set; }
        
        public string ChannelId { get; set; }
        
        public DateTimeOffset CreationDateTime { get; set; }
        
        public DateTimeOffset UpdatedDateTime { get; set; }
        
    }
}