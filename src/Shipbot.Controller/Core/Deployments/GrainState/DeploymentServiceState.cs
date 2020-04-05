using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;
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
        ///     Map storing the planned deployment actions and their associated deployment keys.
        /// </summary>
        public Dictionary<PlannedDeploymentAction, DeploymentKey> PlannedDeploymentActionsIndex { get; } 
            = new Dictionary<PlannedDeploymentAction, DeploymentKey>( PlannedDeploymentAction.EqualityComparer );

        /// <summary>
        ///     Index of deployment IDs per environment
        /// </summary>
        public Dictionary<string, List<DeploymentKey>> EnvironmentalDeployments { get; } = new Dictionary<string, List<DeploymentKey>>();
        
        public HashSet<DeploymentActionKey> DeploymentActions { get; } = new HashSet<DeploymentActionKey>( DeploymentActionKey.EqualityComparer );

        public LatestImageDeployments LatestImageDeployments { get; } = new LatestImageDeployments();
    }
    
    class DictionaryAsArrayResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            if (objectType.GetInterfaces().Any(i => i == typeof(IDictionary) || 
                                                    (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>))))
            {
                return base.CreateArrayContract(objectType);
            }

            return base.CreateContract(objectType);
        }
    }
}