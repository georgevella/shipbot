using System;

namespace Shipbot.Controller.DTOs
{
    public class ContainerImageDto
    {    
        public string Repository { get; set; }
        
        public string Hash { get; set; }
        
        public string Tag { get; set; }
        
        public DateTimeOffset CreationDateTime { get; set; }
    }
}