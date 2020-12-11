namespace Shipbot.ContainerRegistry.Watcher
{
    public class ContainerRepositoryPollingContext
    {
        public ContainerRepositoryPollingContext(string containerRepository)
        {
            ContainerRepository = containerRepository;
        }

        public string ContainerRepository { get; }
    }
}