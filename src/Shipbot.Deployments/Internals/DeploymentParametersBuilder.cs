using System.Collections.Generic;
using Octokit;
using Shipbot.Applications.Models;
using Shipbot.Deployments.Models;

namespace Shipbot.Deployments.Internals
{
    public class DeploymentParametersBuilder
    {
        /// <summary>
        ///     Builds the deployment parameters from various data sources.
        /// </summary>
        /// <param name="deploymentType"></param>
        /// <param name="instanceId"></param>
        /// <param name="imageTagParameters">Map of parameters extracted from the image tag.</param>
        /// <param name="githubPrObject">The github pull request if relevant.</param>
        /// <param name="applicationImage"></param>
        /// <returns></returns>
        public static DeploymentParameters Build(
            DeploymentType deploymentType,
            string instanceId,
            IReadOnlyDictionary<string, string> imageTagParameters,
            PullRequest? githubPrObject,
            ApplicationImage applicationImage)
        {
            var dict = new Dictionary<string, string>(imageTagParameters);
            if (githubPrObject != null)
            {
                dict.Add(DeploymentParameterConstants.PullRequestId, githubPrObject.Id.ToString());
                dict.Add(DeploymentParameterConstants.PullRequestNumber, githubPrObject.Number.ToString());
                dict.Add(DeploymentParameterConstants.PullRequestCreatorEmail, githubPrObject.User.Email);
                
                // TODO: this needs to be handled in such a way that it's not Preview Release specific.
                dict.Add(DeploymentParameterConstants.DeploymentCreator, githubPrObject.User.Email);
            }

            if (applicationImage.Ingress.IsAvailable)
            {
                var domain = applicationImage.Ingress.Domain;

                // TODO we need to change this to support urls instead of domains
                if (deploymentType == DeploymentType.PreviewRelease)
                    domain = $"{instanceId}.{domain}";
                
                dict.Add(DeploymentParameterConstants.Hostname, domain);
            }
            
            return new DeploymentParameters(dict);
        }
    }
}