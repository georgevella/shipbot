using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace Shipbot.Controller.Controllers
{
    [Route("api/images/")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IGrainFactory _grainFactory;

        public ImageController(IClusterClient clusterClient)
        {
            _grainFactory = clusterClient;
        }

        [HttpPost]
        public async Task<ActionResult> SubmitNewImageTag(NewImageTagRequest newImageTagRequest)
        {
            var containerImageGrain = _grainFactory.GetContainerImage(newImageTagRequest.Repository);
            await containerImageGrain.SubmitNewImageTag(newImageTagRequest.Tag);

            return Accepted();
        }
    }

    public class NewImageTagRequest
    {
        public string Repository { get; set; }
        
        public string Tag { get; set; }
    }
}