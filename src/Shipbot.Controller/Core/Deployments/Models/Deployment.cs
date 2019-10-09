using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Deployments.Models
{
    /// <summary>
    ///     Describes a deployment of a container image to an application and one or more of it's environments.
    /// </summary>
    public class Deployment
    {
        private readonly List<DeploymentUpdateDetails> _deploymentUpdates = new List<DeploymentUpdateDetails>();
        
        protected bool Equals(Deployment other)
        {
            return Equals(Application, other.Application) && Equals(ContainerRepository, other.ContainerRepository) && TargetTag == other.TargetTag;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Deployment) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Application != null ? Application.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ContainerRepository != null ? ContainerRepository.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TargetTag != null ? TargetTag.GetHashCode() : 0);
                return hashCode;
            }
        }

        public Application Application { get; }
        public string ContainerRepository { get; }
        public string TargetTag { get; }

        public Deployment(Application application, string containerRepository, string targetTag)
        {
            Application = application;
            ContainerRepository = containerRepository; 
            TargetTag = targetTag;
        }

        public Deployment(DeploymentKey deploymentKey)
            : this(
                deploymentKey.Application,
                deploymentKey.ContainerRepository,
                deploymentKey.TargetTag
            )
        {

        }

        public void AddDeploymentUpdate(DeploymentUpdate deploymentUpdate)
        {
            lock (_deploymentUpdates)
                _deploymentUpdates.Add(
                    new DeploymentUpdateDetails(
                        deploymentUpdate,
                        DeploymentUpdateStatus.Pending
                    )
                );
        }

        public IReadOnlyCollection<DeploymentUpdateDetails> GetDeploymentUpdates()
        {
            lock (_deploymentUpdates)
            {
                var result = _deploymentUpdates.ToImmutableList();
                return result;
            }
        }

        public DeploymentUpdate CreateDeploymentUpdate(ApplicationEnvironment environment, string currentTag, string targetTag, DeploymentUpdate sourceDeploymentUpdate = null)
        {
            return new DeploymentUpdate(Application, environment,
                environment.Images.First(x => x.Repository == ContainerRepository),
                currentTag,
                targetTag,
                sourceDeploymentUpdate);
        }

        public void ChangeDeploymentUpdateStatus(DeploymentUpdate deploymentUpdate, DeploymentUpdateStatus status)
        {
            lock (_deploymentUpdates)
            {
                var details = _deploymentUpdates.First(x => x.DeploymentUpdate.Equals(deploymentUpdate));
                details.DeploymentUpdateStatus = status;
            }
        }
    }
}