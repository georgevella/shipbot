using k8s;
using k8s.Models;
using Newtonsoft.Json;

namespace OperatorSdk.ApiResources
{
    public class ResourceWithMetadata : KubernetesObject
    {
        [JsonProperty("metadata")]
        public V1ObjectMeta Metadata { get; set; }
    }
}