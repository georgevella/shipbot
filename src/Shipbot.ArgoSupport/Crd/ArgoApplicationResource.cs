using OperatorSdk;
using OperatorSdk.ApiResources;

namespace AutoDeploy.ArgoSupport.Crd
{
    [CustomResource("argoproj.io","v1alpha1", "applications")]
    public class ArgoApplicationResource : CustomResourceWithSpecAndStatus<ApplicationSpec, ApplicationStatus>
    {
    }
}