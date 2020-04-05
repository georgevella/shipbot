using System;
using Amazon;
using Amazon.ECR;
using Amazon.ECR.Model;
using Amazon.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shipbot.Controller.Core.Configuration.Registry;

namespace Shipbot.Controller.Core.ContainerRegistry.Clients.Ecr
{
    public class EcrClientFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public EcrClientFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        
        public EcrRegistryClient BuildClient(ContainerRegistrySettings registrySettings)
        {
            var awsCredentials = new BasicAWSCredentials(
                registrySettings.Ecr.AccessKey, 
                registrySettings.Ecr.SecretKey
            );

            var regionEndpoint = RegionEndpoint.GetBySystemName(registrySettings.Ecr.Region);
            var client = new AmazonECRClient(awsCredentials, regionEndpoint);
            
            var repositories = client.DescribeRepositoriesAsync(new DescribeRepositoriesRequest()).Result;
            
            return new EcrRegistryClient( _serviceProvider.GetService<ILogger<EcrRegistryClient>>(), client );
        }
    }
}