using System;

namespace Shipbot.Controller.DTOs
{
    /// <summary>
    ///     Describes an application's deployment information
    /// </summary>
    public class ApplicationDeploymentDto : NewDeploymentDto
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string CurrentTag { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string UpdatePath { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DeploymentStatusDto Status { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public Guid Id { get; set; }
    }
}