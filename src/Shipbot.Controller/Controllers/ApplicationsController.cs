using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Shipbot.Controller.Core.Apps;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Models;
using Shipbot.Controller.RestApiDto;

namespace Shipbot.Controller.Controllers
{
    [Route("api/applications/")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IClusterClient _clusterClient;

        public ValuesController(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }
        
        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<ApplicationDto>>> Get()
        {
            var applicationIndexGrain = _clusterClient.GetApplicationIndexGrain();

            var applications = await applicationIndexGrain.GetAllApplications();

            var result = new List<ApplicationDto>();

            foreach (var appKey in applications)
            {
                var app = _clusterClient.GetApplication(appKey);

                var environments = await app.GetEnvironments();
                result.Add(
                    new ApplicationDto(
                        appKey.Name,
                        environments.Select(x => x.Environment).ToList()
                    )
                );
            }
            
            return Ok(result);
        }
    }
}
