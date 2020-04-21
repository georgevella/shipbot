using System.Collections.Generic;

namespace Shipbot.Controller.RestApiDto
{
    public class ApplicationDto
    {
        public ApplicationDto(string name, List<string> environments)
        {
            Name = name;
            Environments = environments;
        }

        public string Name { get; }
        
        public List<string> Environments { get; }
    }
}