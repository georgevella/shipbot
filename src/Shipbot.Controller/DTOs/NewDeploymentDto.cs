namespace Shipbot.Controller.DTOs
{
    public class NewDeploymentDto
    {
        public string Repository { get; set; }
        public string Tag { get; set; }
        
        public string? Application { get; set; }
        
        public string? Service { get; set; }
    }
}