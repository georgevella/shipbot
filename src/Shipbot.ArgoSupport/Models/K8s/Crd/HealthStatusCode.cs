namespace AutoDeploy.ArgoSupport.Models.K8s.Crd
{
    public enum HealthStatusCode
    {
        Unknown,
        Progressing,
        Healthy,
        Suspended,
        Degraded,
        Missing,
    }
}