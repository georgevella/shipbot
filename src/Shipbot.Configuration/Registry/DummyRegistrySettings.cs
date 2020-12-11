using System;
using System.Collections.Generic;
// ReSharper disable CollectionNeverUpdated.Global

namespace Shipbot.Controller.Core.Configuration.Registry
{
    public class DummyRegistrySettings
    {
        public Dictionary<string, DummyImageRepository> Repositories { get; } =
            new Dictionary<string, DummyImageRepository>();
    }

    public class DummyImageRepository
    {
        public List<DummyContainerImage> Images { get; } = new List<DummyContainerImage>();

        public DummyImageRepositoryGeneratorSettings Generate { get; } = new DummyImageRepositoryGeneratorSettings();
    }

    public class DummyImageRepositoryGeneratorSettings
    {
        
    }

    public class DummyContainerImage
    {
        public string Tag { get; set; }
        
        public string Hash { get; set; }
        
        public DateTimeOffset CreationDateTime { get; set; } 
    }
}