using System.Collections.Generic;
using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    public class ApplicationSourceHelm
    {
        protected bool Equals(ApplicationSourceHelm other)
        {
            return Equals(FileParameters, other.FileParameters) && 
                   Equals(Parameters, other.Parameters) && 
                   ReleaseName == other.ReleaseName &&
                   Equals(ValueFiles, other.ValueFiles) && 
                   Values == other.Values && 
                   Version == other.Version;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationSourceHelm) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (FileParameters != null ? FileParameters.GetCollectionHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Parameters != null ? Parameters.GetCollectionHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ReleaseName != null ? ReleaseName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ValueFiles != null ? ValueFiles.GetCollectionHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Values != null ? Values.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Version != null ? Version.GetHashCode() : 0);
                return hashCode;
            }
        }

        [JsonConstructor]
        public ApplicationSourceHelm(
            List<ApplicationSourceHelmFileParameter> fileParameters, 
            List<ApplicationSourceHelmParameter> parameters, 
            string releaseName, 
            List<string> valueFiles, 
            string values, 
            string version
        )
        {
            FileParameters = fileParameters;
            Parameters = parameters;
            ReleaseName = releaseName;
            ValueFiles = valueFiles;
            Values = values;
            Version = version;
        }

        [JsonProperty("fileParameters")] public List<ApplicationSourceHelmFileParameter> FileParameters { get; }
        [JsonProperty("parameters")] public List<ApplicationSourceHelmParameter> Parameters { get; }
        [JsonProperty("releaseName")] public string ReleaseName { get; }
        [JsonProperty("valueFiles")] public List<string> ValueFiles { get; }
        [JsonProperty("values")] public string Values { get; }
        [JsonProperty("version")] public string Version { get; }
    }
}