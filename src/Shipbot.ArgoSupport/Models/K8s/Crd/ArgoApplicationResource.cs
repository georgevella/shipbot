using OperatorSdk;
using OperatorSdk.ApiResources;

namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    [CustomResource("argoproj.io", version: "v1alpha1", "applications")]
    public class ArgoApplicationResource : CustomResourceWithSpecAndStatus<ApplicationSpec, ApplicationStatus> 
    {
    }
}