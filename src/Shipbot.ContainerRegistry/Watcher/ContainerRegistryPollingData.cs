namespace Shipbot.ContainerRegistry.Watcher
{
    public class ContainerRegistryPollingData
    {
        public ContainerRegistryPollingData(string imageRepository, string applicationId, int imageIndex)
        {
            ImageRepository = imageRepository;
            ApplicationId = applicationId;
            ImageIndex = imageIndex;
        }

        public string ImageRepository { get; }
        public string ApplicationId { get; }
        public int ImageIndex { get; }
    }
}