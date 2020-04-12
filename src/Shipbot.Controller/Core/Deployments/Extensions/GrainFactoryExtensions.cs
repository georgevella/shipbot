using System;
using Orleans;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Deployments.GrainKeys;
using Shipbot.Controller.Core.Deployments.Grains;
using Shipbot.Controller.Core.Deployments.GrainState;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.Core.Models;
using Shipbot.Controller.Core.Utilities;

// ReSharper disable once CheckNamespace
namespace Orleans
{
    public static partial class GrainFactoryExtensions
    {
        // public static IDeploymentGrain GetDeploymentGrain(this IGrainFactory grainFactory, ApplicationEnvironmentKey applicationEnvironmentKey, Image image, string targetTag)
        // {
        //    return grainFactory.GetGrain<IDeploymentGrain>(new DeploymentKey(applicationEnvironmentKey, image, targetTag));
        // }
        //
        public static IDeploymentQueueGrain GetDeploymentQueueGrain(this IGrainFactory grainFactory)
        {
            return grainFactory.GetGrain<IDeploymentQueueGrain>(DeploymentGrainKeyConstants.DeploymentQueueGrainKey);
        }
        //
        public static IDeploymentGrain GetDeploymentGrain(this IGrainFactory grainFactory, DeploymentKey deploymentKey)
        {
            return grainFactory.GetGrain<IDeploymentGrain>(deploymentKey);
        }
        
        public static IDeploymentActionGrain GetDeploymentActionGrain(this IGrainFactory grainFactory, DeploymentActionKey deploymentActionKey)
        {
            return grainFactory.GetGrain<IDeploymentActionGrain>(deploymentActionKey);
        }
        
        public static IDeploymentServiceGrain GetDeploymentServiceGrain(this IGrainFactory grainFactory, ApplicationKey applicationKey)
        {
            return grainFactory.GetGrain<IDeploymentServiceGrain>(applicationKey.Name);
        }
    }
}