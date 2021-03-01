using System.Collections.Generic;

namespace Shipbot.Controller.Core.Configuration.Apps
{
    public class ImageSettings
    {
        public string Pattern { get; set; }

        public string Repository { get; set; }

        public TagPropertySettings TagProperty { get; set; }

        public UpdatePolicy Policy { get; set; }
        
        public IngressSettings? Ingress { get; set; }
        
        public ImagePreviewReleaseSettings? PreviewRelease { get; set; }
        
        public ApplicationSourceCodeSettings? ApplicationSource { get; set; }
    }

    public class IngressSettings
    {
        public string Domain { get; set; }
    }

    public class ApplicationSourceCodeSettings
    {
        public GithubApplicationSourceCodeSettings? Github { get; set; }
    }

    public class GithubApplicationSourceCodeSettings
    {
        public string Owner { get; set; }
        
        public string Repository { get; set; }
    }

    public class ImagePreviewReleaseSettings
    {
        public ImageUpdatePolicySettings UpdatePolicy { get; set; } = new ImageUpdatePolicySettings();

        public ImagePreviewReleaseParameterSettings Parameters { get; set; } =
            new ImagePreviewReleaseParameterSettings();
    }

    public class ImagePreviewReleaseParameterSettings
    {
        public Dictionary<string, string> Static { get; set; } = new Dictionary<string, string>();

        public ImagePreviewReleaseParameterRegexSettings Regex { get; set; } =
            new ImagePreviewReleaseParameterRegexSettings();
    }

    public class ImagePreviewReleaseParameterRegexSettings
    {
        public string Tag { get; set; }
    }

    public class ImageUpdatePolicySettings
    {
        public UpdatePolicy Type { get; set; }
        
        public string Pattern { get; set; }
    }

    public class TagPropertySettings
    {
        public string Path { get; set; }

        public TagPropertyValueFormat ValueFormat { get; set; }
    }
    
    public enum TagPropertyValueFormat
    {
        TagOnly,
        TagAndRepository
    }
}