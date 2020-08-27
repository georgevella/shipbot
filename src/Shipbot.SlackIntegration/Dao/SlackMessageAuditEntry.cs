using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shipbot.Data;

namespace Shipbot.SlackIntegration.Dao
{
    [Table("slackMessageAudit")]
    public class SlackMessageAuditEntry : BaseLazyLoadedPoco
    {
        private SlackMessage? _message;

        [Key]
        public Guid Id { get; set; }

        public SlackMessage? Message
        {
            get => LazyLoader.Load( this, ref _message);
            set => _message = value;
        }
        public Guid MessageId { get; set; }
        
        public DateTimeOffset CreationDateTime { get; set; }
        
        public string Timestamp { get; set; }
        
        public string ChannelId { get; set; }
        
        public SlackMessageAuditEntryType Type { get; set; }
    }

    public enum SlackMessageAuditEntryType
    {
        Created,
        Editted,
        Deleted,
        UserInteraction
    }
}