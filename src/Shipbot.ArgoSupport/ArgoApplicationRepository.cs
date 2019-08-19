using System.Collections.Concurrent;
using ArgoAutoDeploy.Core.Argo.Crd;

namespace AutoDeploy.ArgoSupport
{
    public class ArgoApplicationRepository
    {
        private ConcurrentDictionary<string, ArgoApplicationResource> _argoApplications = new ConcurrentDictionary<string, ArgoApplicationResource>();
        
        
    }
}