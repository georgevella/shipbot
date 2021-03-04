namespace Shipbot.Controller.Core.Configuration.Apps
{
    public enum UpdatePolicy
    {
        All,
        Major,
        Minor,
        Patch,
        Tag,
        Glob,
        Regex,
        Semver
    }
}