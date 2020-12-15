using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shipbot.Applications;
using Shipbot.Contracts;
using Shipbot.Controller.Core.ApplicationSources;
using Shipbot.Controller.Core.ApplicationSources.Models;
using Shipbot.Controller.DTOs;
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
        [Authorize]
        public async Task<ActionResult<IEnumerable<ApplicationSourceDto>>> Get()
        {
            var activeAppications = await _applicationSourceService.GetActiveApplications();

            return Ok(
                activeAppications
                    .Select(applicationSource => new ApplicationSourceDto()
                    {
                        Path = applicationSource.Path,
                        Ref = applicationSource.Repository.Ref,
                        Uri = applicationSource.Repository.Uri.ToString()
                    })
                    .ToList()
            );
        }
    }
}
