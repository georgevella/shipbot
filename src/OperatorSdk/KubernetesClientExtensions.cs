using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;

namespace OperatorSdk
{
    public static class KubernetesClientExtensions
    {
        public static async Task<T> GetNamespacedCustomResource<T>(this IKubernetes client, 
            string ns,
            string name,
            CancellationToken cancellationToken
            )
            where T : KubernetesObject
        {
            var result = await client.GetNamespacedCustomResourceWithHttpMessagesAsync<T>(
                ns, 
                name, 
                CancellationToken.None
                );

            return result.Body;
        }
        
        public static async Task<HttpOperationResponse<T>> GetNamespacedCustomResourceWithHttpMessagesAsync<T>(this IKubernetes client, 
            string ns,
            string name,
            CancellationToken cancellationToken
        )
            where T : KubernetesObject
        {
            var customResourceDetails = typeof(T).GetTypeInfo().GetCustomAttribute<CustomResourceAttribute>();

            var result = await client.GetNamespacedCustomObjectWithHttpMessagesAsync(
                customResourceDetails.ApiGroup,
                customResourceDetails.Version,
                ns,
                customResourceDetails.Plural,
                name,
                cancellationToken: cancellationToken);

            if (!(result.Body is JObject jObject))
                throw new InvalidOperationException("Returned data is not of the correct type.");
            
            var entity = jObject.ToObject<T>();
            return new HttpOperationResponse<T>()
            {
                Body = entity,
                Request = result.Request,
                Response = result.Response
            };
        }
    }
}