using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Deployments.GrainKeys;
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
        public Dictionary<(string environment, string imageRepository, string imageTagValuePath, string targetTag), DeploymentKey> PlannedDeploymentActionsIndex { get; } 
            = new Dictionary<(string environment, string imageRepository, string imageTagValuePath, string targetTag), DeploymentKey>( new PlannedDeploymentActionsEqualityComparer() );
    }

    public class PlannedDeploymentActionsEqualityComparer : IEqualityComparer<(string environment, string imageRepository, string imageTagValuePath, string targetTag)>
    {
        public bool Equals(
            (string environment, string imageRepository, string imageTagValuePath, string targetTag) x,
            (string environment, string imageRepository, string imageTagValuePath, string targetTag) y
            )
        {
            return x.environment == y.environment &&
                   x.imageRepository == y.imageRepository &&
                   x.imageTagValuePath == y.imageTagValuePath &&
                   x.targetTag == y.targetTag;
        }

        public int GetHashCode((string environment, string imageRepository, string imageTagValuePath, string targetTag) obj)
        {
            var hashCode = 864;
            
            hashCode = (hashCode * 397) ^ (obj.environment != null ? obj.environment.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (obj.imageRepository != null ? obj.imageRepository.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (obj.imageTagValuePath != null ? obj.imageTagValuePath.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (obj.targetTag != null ? obj.targetTag.GetHashCode() : 0);

            return hashCode;
        }
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