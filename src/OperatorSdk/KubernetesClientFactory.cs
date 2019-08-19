using System;
using System.Collections.Generic;
using System.Diagnostics;
using ArgoAutoDeploy.Core.Configuration.K8s;
using k8s;
using k8s.KubeConfigModels;
using Newtonsoft.Json;

namespace OperatorSdk
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class KubernetesClientFactory
    {
        public Kubernetes Create(KubernetesConnectionDetails connectionDetails)
        {
            var config = connectionDetails.Mode switch {
                KubernetesConnectionMode.InCluster => KubernetesClientConfiguration.InClusterConfig(),
                KubernetesConnectionMode.EKS => BuildEksConfiguration(connectionDetails),
                KubernetesConnectionMode.File => KubernetesClientConfiguration.BuildConfigFromConfigFile(
                    connectionDetails.File.Filepath,
                    connectionDetails.File.Context
                )
                
                };

            return new Kubernetes(config);
        }

        private KubernetesClientConfiguration BuildEksConfiguration(KubernetesConnectionDetails connectionDetails)
        {
            var processStartOptions = new ProcessStartInfo()
            {
                Arguments = $"token -i {connectionDetails.Eks.Name}",
                FileName = "aws-iam-authenticator",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                Environment =
                {
                    new KeyValuePair<string, string>("AWS_ACCESS_KEY_ID", connectionDetails.Eks.AccessKey),
                    new KeyValuePair<string, string>("AWS_SECRET_ACCESS_KEY", connectionDetails.Eks.SecretKey),
                    new KeyValuePair<string, string>("AWS_DEFAULT_REGION", connectionDetails.Eks.Region)
                }
            };
            var processInfo = Process.Start(processStartOptions);
            processInfo.WaitForExit();
            Debug.Assert(processInfo.ExitCode == 0);


            var jsonOutput = processInfo.StandardOutput.ReadToEnd();
            var response = JsonConvert.DeserializeObject<K8sAuthTokenResponse>(jsonOutput);


            var c = new K8SConfiguration
            {
                CurrentContext = connectionDetails.Eks.Name,
                Clusters = new[]
                {
                    new Cluster
                    {
                        Name = connectionDetails.Eks.Name,
                        ClusterEndpoint = new ClusterEndpoint
                        {
                            CertificateAuthorityData = connectionDetails.Eks.CA,
                            Server = connectionDetails.Eks.Endpoint
                        }
                    }
                },
                Contexts = new[]
                {
                    new Context()
                    {
                        Name = connectionDetails.Eks.Name,
                        ContextDetails = new ContextDetails()
                        {
                            Cluster = connectionDetails.Eks.Name,
                            User = "user"
                        }
                    }
                },
                Users = new[]
                {
                    new User
                    {
                        Name = "user",
                        UserCredentials = new UserCredentials
                        {
                            Token = response.Status.Token
                        }
                    }
                }
            };
            return KubernetesClientConfiguration.BuildConfigFromConfigObject(c);
        }
    }

    internal class K8sAuthTokenResponse
    {
        public string Kind { get; set; }

        public K8sAuthTokenStatus Status { get; set; }
    }

    internal class K8sAuthTokenStatus
    {
        public DateTime ExpirationTimestamp { get; set; }
        public string Token { get; set; }
    }
}