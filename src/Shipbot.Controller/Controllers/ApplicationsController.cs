using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Util.Internal.PlatformServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shipbot.Applications;
using Shipbot.Controller.Core.ApplicationSources;
using Shipbot.Controller.DTOs;
using Shipbot.Models;

namespace Shipbot.Controller.Controllers
{
    [Route("api/applications")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;
        private readonly IApplicationSourceService _applicationSourceService;

        public ApplicationsController(
            IApplicationService applicationService,
            IApplicationSourceService applicationSourceService
            
            )
        {
            _applicationService = applicationService;
            _applicationSourceService = applicationSourceService;
        }
        
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ApplicationDto>>> Get()
        {
            var apps = _applicationService.GetApplications();
            var result = new List<ApplicationDto>();
            foreach (var app in apps)
            {
                var dto = await ConvertApplicationToDto(app);
                result.Add(dto);
            }
            return Ok(result);
        }
        
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ApplicationDto>> Get(string id)
        {
            try
            {
                var application = _applicationService.GetApplication(id);
                var dto = await ConvertApplicationToDto(application);
                return Ok(dto);
            }
            catch (Exception e)
            {
                return NotFound();
            }
        }

        [HttpGet("{id}/services/")]
        public async Task<ActionResult<ApplicationServiceDto>> GetApplicationServices(string id)
        {
            try
            {
                var application = _applicationService.GetApplication(id);
                var applicationImages = application.Images
                    .Select(ConvertApplicationImageToApplicationServiceDto)
                    .ToList();
                return Ok(applicationImages);
            }
            catch (Exception e)
            {
                return NotFound();
            }
        }

        private async Task<ApplicationDto> ConvertApplicationToDto(Application application)
        {
            var sources = await _applicationSourceService.GetActiveApplications();

            var applicationSource = sources.FirstOrDefault(
                x => x.Application.Equals(application.Name)
            );
            var applicationImages = application.Images
                .Select(
                    image => ConvertApplicationImageToApplicationServiceDto(image)
                )
                .ToList();

            var dto = new ApplicationDto()
            {
                Name = application.Name,
                Source = applicationSource != null
                    ? new ApplicationSourceDto()
                    {
                        Path = applicationSource.Path,
                        Ref = applicationSource.Repository.Ref,
                        Uri = applicationSource.Repository.Uri.ToString()
                    }
                    : new ApplicationSourceDto(),
                Services = applicationImages
            };
            return dto;
        }

        private static ApplicationServiceDto ConvertApplicationImageToApplicationServiceDto(ApplicationImage image)
        {
            var item = new ApplicationServiceDto()
            {
                ContainerRepository = image.Repository,
                DeploymentSettings = new DeploymentSettingsDto()
                {
                    TagProperty = new TagPropertyDto()
                    {
                        Path = image.TagProperty.Path,
                        ValueFormat = image.TagProperty.ValueFormat
                    },
                    Policy = image.Policy switch
                    {
                        GlobImageUpdatePolicy globImageUpdatePolicy => new ImageUpdatePolicyDto()
                        {
                            Glob = new GlobImageUpdatePolicyDto()
                            {
                                Pattern = globImageUpdatePolicy.Pattern
                            }
                        },
                        RegexImageUpdatePolicy regexImageUpdatePolicy => new ImageUpdatePolicyDto()
                        {
                            Regex = new RegexImageUpdatePolicyDto()
                            {
                                Pattern = regexImageUpdatePolicy.Pattern
                            }
                        },
                        SemverImageUpdatePolicy semverImageUpdatePolicy => new ImageUpdatePolicyDto()
                        {
                            Semver = new SemverImageUpdatePolicyDto()
                        },
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    AutomaticallySubmitDeploymentToQueue = image.DeploymentSettings.AutomaticallySubmitDeploymentToQueue,
                    AutomaticallyCreateDeploymentOnRepositoryUpdate =
                        image.DeploymentSettings.AutomaticallyCreateDeploymentOnRepositoryUpdate
                },
            };
            return item;
        }

        [HttpGet("{id}/current-tags")]
        [Authorize]
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
