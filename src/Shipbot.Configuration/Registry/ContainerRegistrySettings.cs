namespace Shipbot.Controller.Core.Configuration.Registry
{
    public class ContainerRegistrySettings
    {
        public string Name { get; set; }
        
        public ContainerRegistryType Type { get; set; }
        
        public DockerRegistrySettings DockerRegistry { get; set; }
        
        public EcrSettings Ecr { get; set; }
        
        public DummyRegistrySettings Dummy { get; set; }
    }
}