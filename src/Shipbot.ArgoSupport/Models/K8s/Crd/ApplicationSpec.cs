using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    public class ApplicationSpec
    {
        protected bool Equals(ApplicationSpec other)
        {
            return Equals(ApplicationDestination, other.ApplicationDestination) && 
                   IgnoreDifferences.IsCollectionEqualTo(other.IgnoreDifferences) && 
                   Equals(Info, other.Info) && 
                   Project == other.Project && 
                   Equals(ApplicationSource, other.ApplicationSource) && 
                   Equals(ArgoApplicationSyncPolicy, other.ArgoApplicationSyncPolicy);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationSpec) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ApplicationDestination != null ? ApplicationDestination.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (IgnoreDifferences != null ? IgnoreDifferences.GetCollectionHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Info != null ? Info.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Project != null ? Project.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ApplicationSource != null ? ApplicationSource.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ArgoApplicationSyncPolicy != null ? ArgoApplicationSyncPolicy.GetHashCode() : 0);
                return hashCode;
            }
        }

        public ApplicationSpec(
            ApplicationDestination applicationDestination, 
            List<object> ignoreDifferences, 
            List<BaseNameValueModel> info, 
            string project, 
            ApplicationSource applicationSource, 
            ArgoApplicationSyncPolicy argoApplicationSyncPolicy
            )
        {
            ApplicationDestination = applicationDestination;
            IgnoreDifferences = ignoreDifferences;
            Info = info;
            Project = project;
            ApplicationSource = applicationSource;
            ArgoApplicationSyncPolicy = argoApplicationSyncPolicy;
        }

        [JsonProperty("destination")] public ApplicationDestination ApplicationDestination { get; }
        [JsonProperty("ignoreDifferences")] public List<object> IgnoreDifferences { get; }
        [JsonProperty("info")] public List<BaseNameValueModel> Info { get; }
        [JsonProperty("project")] public string Project { get; }
        [JsonProperty("source")]  public ApplicationSource ApplicationSource { get; }
        [JsonProperty("syncPolicy")] public ArgoApplicationSyncPolicy ArgoApplicationSyncPolicy { get; }
    }
}