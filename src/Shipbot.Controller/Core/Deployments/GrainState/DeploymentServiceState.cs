using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Deployments.Models;

namespace Shipbot.Controller.Core.Deployments.GrainState
{
    public class DeploymentServiceState
    {
        /// <summary>
        ///    List of all deployments linked to an application. 
        /// </summary>
        public HashSet<DeploymentKey> Deployments { get; } = new HashSet<DeploymentKey>(DeploymentKey.EqualityComparer);

        /// <summary>
        ///     Index of deployment IDs per environment
        /// </summary>
        public Dictionary<string, List<DeploymentKey>> EnvironmentalDeployments { get; } = new Dictionary<string, List<DeploymentKey>>();
        
        public HashSet<DeploymentActionKey> DeploymentUpdates { get; } = new HashSet<DeploymentActionKey>( DeploymentActionKey.EqualityComparer );

        public LatestImageDeployments LatestImageDeployments { get; } = new LatestImageDeployments();
    }
}