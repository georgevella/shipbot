using System.Collections.Generic;

namespace Shipbot.Controller.Core.ContainerRegistry.Models
{
    public class ImageTagCollection : HashSet<ImageTag>
    {
        public ImageTagCollection() : base(ImageTag.EqualityComparer)
        {
            
        }
    }
}