using System.Diagnostics;
using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    [DebuggerDisplay("Name: {Name}, Kind: {Kind}, Status: {Status}, Health: {Health}")]
    public class ArgoApplicationRelatedResourceStatus : ArgoApplicationRelatedResource
    {
        [JsonConstructor]
        public ArgoApplicationRelatedResourceStatus(
            string name, 
            string @group, 
            string kind, 
            string @namespace, 
            ResourceHealthStatus health, 
            bool hook, 
            bool requiresPruning, 
            SyncStatusCode status, 
            string version
        ) : base(name, @group, kind, @namespace)
        {
            Health = health;
            Hook = hook;
            RequiresPruning = requiresPruning;
            Status = status;
            Version = version;
        }

        [JsonProperty("health")] public ResourceHealthStatus Health { get; set; }
        [JsonProperty("hook")] public bool Hook { get; set; }
        [JsonProperty("requiresPruning")] public bool RequiresPruning { get; set; }
        [JsonProperty("status")] public SyncStatusCode Status { get; set; }
        [JsonProperty("version")] public string Version { get; set; }
    }
}