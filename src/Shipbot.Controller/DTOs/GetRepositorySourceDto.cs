namespace Shipbot.Controller.DTOs
{
    public class GetRepositorySourceDto
    {
        public string Uri { get; set; }
        
        public string Ref { get; set; }
        
        public string Path { get; set; }
    }
}