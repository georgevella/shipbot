using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Shipbot.Applications;
using Shipbot.Models;

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
            try
            {
                return Ok(_applicationService.GetApplication(id));
            }
            catch (Exception e)
            {
                return NotFound();
            }
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
