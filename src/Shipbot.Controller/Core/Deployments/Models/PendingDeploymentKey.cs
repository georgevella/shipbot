using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Deployments.Models
{
    internal class PendingDeploymentKey
    {
        protected bool Equals(PendingDeploymentKey other)
        {
            return Equals(Application, other.Application) && Equals(ApplicationEnvironment, other.ApplicationEnvironment);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PendingDeploymentKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Application != null ? Application.GetHashCode() : 0) * 397) ^ (ApplicationEnvironment != null ? ApplicationEnvironment.GetHashCode() : 0);
            }
        }

        public Application Application { get; }
        public ApplicationEnvironment ApplicationEnvironment { get; }

        public PendingDeploymentKey(Application application, ApplicationEnvironment applicationEnvironment)
        {
            Application = application;
            ApplicationEnvironment = applicationEnvironment;
        }
    }
}