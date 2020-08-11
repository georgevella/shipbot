using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Shipbot.Controller.Core.Apps;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Controllers
{
    [Route("api/applications")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public ApplicationsController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }
        
        [HttpGet]
        public ActionResult<IEnumerable<Application>> Get()
        {
            return Ok(_applicationService.GetApplications());
        }
        
        [HttpGet("{id}")]
        public ActionResult<Application> Get(string id)
        {
            return Ok(_applicationService.GetApplication(id));
        }
        
        [HttpGet("{id}/current-tags")]
        public ActionResult<Dictionary<string, string>> GetCurrentImageTags(string id)
        {
            var application = _applicationService.GetApplication(id);
            var result = _applicationService.GetCurrentImageTags(application)
                .ToDictionary(
                    x => x.Key.TagProperty.Path, 
                    x => x.Key.TagProperty.ValueFormat == TagPropertyValueFormat.TagOnly ? $"{x.Key.Repository}:{x.Value}" : $"{x.Value}"
                    );
            return Ok(result);
        }
    }
}
