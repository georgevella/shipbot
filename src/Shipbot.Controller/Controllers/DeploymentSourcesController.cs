using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shipbot.Applications;
using Shipbot.Contracts;
using Shipbot.Controller.Core.ApplicationSources;
using Shipbot.Models;

namespace Shipbot.Controller.Controllers
{
    [Route("api/deployment-sources")]
    [ApiController]
    public class DeploymentSourcesController : ControllerBase
    {
        private readonly IApplicationSourceService _applicationSourceService;

        public DeploymentSourcesController(IApplicationSourceService applicationSourceService)
        {
            _applicationSourceService = applicationSourceService;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Application>>> Get()
        {
            var activeAppications = await _applicationSourceService.GetActiveApplications();
            
            return Ok(activeAppications);
        }
    }
}
