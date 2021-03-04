using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Util.Internal.PlatformServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Octokit;
using Shipbot.Applications;
using Shipbot.Applications.Models;
using Shipbot.Controller.Core.ApplicationSources;
using Shipbot.Controller.DTOs;
using Shipbot.Models;
using Application = Shipbot.Applications.Models.Application;

namespace Shipbot.Controller.Controllers
{
    [Route("api/applications")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;
        private readonly IDeploymentManifestSourceService _deploymentManifestSourceService;
        private readonly IApplicationImageInstanceService _applicationImageInstanceService;

        public ApplicationsController(
            IApplicationService applicationService,
            IDeploymentManifestSourceService deploymentManifestSourceService,
            IApplicationImageInstanceService applicationImageInstanceService
            )
        {
            _applicationService = applicationService;
            _deploymentManifestSourceService = deploymentManifestSourceService;
            _applicationImageInstanceService = applicationImageInstanceService;
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
        public async Task<ActionResult<ApplicationImageDto>> GetApplicationServices(string id)
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
            var sources = await _deploymentManifestSourceService.GetActiveApplications();

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
                DeploymentManifestSource = applicationSource != null
                    ? new GetRepositorySourceDto()
                    {
                        Path = applicationSource.Path,
                        Ref = applicationSource.Repository.Ref,
                        Uri = applicationSource.Repository.Uri.ToString()
                    }
                    : new GetRepositorySourceDto(),
                Services = applicationImages,
                
            };
            return dto;
        }

        private static ApplicationImageDto ConvertApplicationImageToApplicationServiceDto(ApplicationImage image)
        {
            var imagePreviewReleaseConfig = image.DeploymentSettings.PreviewReleases;
            var previewReleaseSettingsDto = imagePreviewReleaseConfig.Enabled
                ? new PreviewReleaseSettingsDto()
                {
                    Enabled = true,
                    UpdatePolicy = imagePreviewReleaseConfig.Policy switch
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
                            {
                                Constraint = semverImageUpdatePolicy.Constraint
                            }
                        },
                        _ => throw new ArgumentOutOfRangeException()
                    }
                }
                : new PreviewReleaseSettingsDto()
                {
                    Enabled = false
                };
            
            var item = new ApplicationImageDto()
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
                        image.DeploymentSettings.AutomaticallyCreateDeploymentOnImageRepositoryUpdate,
                    PreviewRelease = previewReleaseSettingsDto
                },
            };
            return item;
        }

        [HttpGet("{id}/current-tags/{instanceId?}")]
        [Authorize]
        public async Task<ActionResult<Dictionary<string, string>>> GetCurrentImageTags(string id, string instanceId = "")
        {
            var application = _applicationService.GetApplication(id);
            
            var instanceIds = instanceId.ToLower() switch
            {
                "prime" => new List<string>() {""},
                "primary" => new List<string>() {""},
                "" => _applicationImageInstanceService.GetAllInstanceIdsForApplication(application),
                _ => new List<string>() {instanceId}
            };
            var result = new Dictionary<string, Dictionary<string, string>>();
            foreach (var x in instanceIds)
            {
                var instanceTags = new Dictionary<string, string>();
                result.Add(string.IsNullOrEmpty(x) ? "primary" : x, instanceTags);
                
                foreach (var image in application.Images)
                {
                    var currentTag = await _applicationImageInstanceService.GetCurrentTag(application, image, x);
                    if (!currentTag.available)
                    {
                        instanceTags[image.TagProperty.Path] = "not-available-yet";
                    }
                    else
                    {
                        instanceTags[image.TagProperty.Path] = image.TagProperty.ValueFormat == TagPropertyValueFormat.TagOnly
                            ? $"{image.Repository}:{currentTag.tag}"
                            : $"{currentTag.tag}";
                    }
                }
            }

            if (result.Count == 1)
            {
                // single entry mode!
                return Ok(result.First().Value);
            }

            if (result.Count > 1)
            {
                // multi-entry mode
                return Ok(result);
            }
            
            return NotFound();
        }
    }
}
