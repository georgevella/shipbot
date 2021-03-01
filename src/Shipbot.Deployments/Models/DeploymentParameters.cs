using System.Collections.Generic;

namespace Shipbot.Deployments.Models
{
    public class DeploymentParameters
    {
        private readonly Dictionary<string, string> _parameterStore;

        public DeploymentParameters(Dictionary<string, string> parameterStore)
        {
            _parameterStore = parameterStore;
        }

        public static implicit operator Dictionary<string, string>(DeploymentParameters parameters)
        {
            return parameters._parameterStore;
        }

        public IReadOnlyDictionary<string, string> ToDictionary() => _parameterStore;

        public string? Branch => _parameterStore.GetValueOrDefault(DeploymentParameterConstants.PreviewReleaseBranch);
    }
}