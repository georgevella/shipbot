using OperatorSdk;
using OperatorSdk.ApiResources;

namespace ArgoAutoDeploy.Core.Argo.Crd
{
    [CustomResource("argoproj.io","v1alpha1", "applications")]
    public class ArgoApplicationResource : CustomResourceWithSpecAndStatus<ApplicationSpec, ApplicationStatus>
    {
    }
}