namespace Shipbot.Controller.Core.Slack.Models
{
    public class SlackPromoteActionDetails
    {
        public string Application { get; set; }
        
        public string ContainerRepository { get; set; }
        
        public string TargetTag { get; set; }
        
        public string SourceEnvironment { get; set; }
    }
}