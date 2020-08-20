using System.Collections.Concurrent;
using AutoDeploy.ArgoSupport.Crd;

namespace AutoDeploy.ArgoSupport
{
    public class ArgoApplicationRepository
    {
        private ConcurrentDictionary<string, ArgoApplicationResource> _argoApplications = new ConcurrentDictionary<string, ArgoApplicationResource>();
        
        
    }
}