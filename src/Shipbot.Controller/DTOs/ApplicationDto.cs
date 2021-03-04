using System.Collections.Generic;
using Newtonsoft.Json;
using Shipbot.Applications.Models;
using Shipbot.Models;

namespace Shipbot.Controller.DTOs
{
    public class ApplicationDto
    {
        public string Name { get; set; }
        
        public GetRepositorySourceDto DeploymentManifestSource { get; set; }
        public List<ApplicationImageDto> Services { get; set; }
    }

    public class ApplicationImageDto
    {
        public string ContainerRepository { get; set; }

        public DeploymentSettingsDto DeploymentSettings { get; set; }
    }

    public class DeploymentSettingsDto
    {
        public TagPropertyDto TagProperty { get; set; }

        public ImageUpdatePolicyDto Policy { get; set; }
        
        public bool AutomaticallyCreateDeploymentOnRepositoryUpdate { get; set; }
        
        public bool AutomaticallySubmitDeploymentToQueue { get; set; }
        
        public PreviewReleaseSettingsDto PreviewRelease { get; set; }
    }

    public class PreviewReleaseSettingsDto
    {
        public bool Enabled { get; set; }
        
        public ImageUpdatePolicyDto UpdatePolicy { get; set; }
    }

    public class ImageUpdatePolicyDto
    {
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public GlobImageUpdatePolicyDto Glob { get; set; }
        
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public RegexImageUpdatePolicyDto Regex { get; set; }
        
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public SemverImageUpdatePolicyDto Semver { get; set; }
    }

    public class SemverImageUpdatePolicyDto
    {
        public string Constraint { get; set; }
    }

    public class RegexImageUpdatePolicyDto
    {        
        public string Pattern { get; set; }
    }

    public class GlobImageUpdatePolicyDto
    {
        public string Pattern { get; set; }
    }

    public class TagPropertyDto
    {
        public string Path { get; set; }
        public TagPropertyValueFormat ValueFormat { get; set; }
    }
}