using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.ECR;
using Amazon.ECR.Model;
using Microsoft.Extensions.Logging;
using Shipbot.ContainerRegistry.Models;

namespace Shipbot.ContainerRegistry.Ecr
{
    public class EcrRegistryClient : IRegistryClient
    {
        private readonly ILogger<EcrRegistryClient> _log;
        private readonly AmazonECRClient _client;

        public EcrRegistryClient(
            ILogger<EcrRegistryClient> log,
            AmazonECRClient client
            )
        {
            _log = log;
            _client = client;
        }
        
        public async Task<bool> IsKnownRepository(string repository)
        {
            var repositories = await GetRepositories();

            return repositories.Any(
                r => r.Equals(repository, StringComparison.OrdinalIgnoreCase)
            );
        }

        private async Task<List<(string repositoryUri, string registryId, string repositoryName)>> GetRepositoriesInternal()
        {
            DescribeRepositoriesResponse? ecrResponse = null;
            var result = new List<(string repositoryUri, string registryId, string repositoryName)>();
            
            do
            {
                ecrResponse = await _client.DescribeRepositoriesAsync(new DescribeRepositoriesRequest()
                {
                    MaxResults = 1000,
                    NextToken = (ecrResponse?.NextToken) ?? null
                    
                });
                
                result.AddRange(ecrResponse.Repositories.Select(r => (r.RepositoryUri, r.RegistryId, r.RepositoryName) ));
            }
            while (ecrResponse.NextToken != null);

            return result;
        }

        public async Task<IEnumerable<string>> GetRepositories()
        {
            var result = await GetRepositoriesInternal();
            return result.Select(x => x.repositoryUri).ToList();
        }

        public async Task<ContainerImage> GetImage(string repository, string tag)
        {
            var repositories = await GetRepositoriesInternal();
            var repo = repositories.First(x => x.repositoryUri == repository);
            
            var describeImagesRequest = new DescribeImagesRequest()
            {
                ImageIds = new List<ImageIdentifier>()
                {
                    new ImageIdentifier()
                    {
                        ImageTag = tag
                    }
                },
                RegistryId = repo.registryId,
                RepositoryName = repo.repositoryName,
            };
                
            var images = await _client.DescribeImagesAsync(describeImagesRequest, CancellationToken.None);
            return images.ImageDetails.SelectMany(
                    i => i.ImageTags,
                    (i, tag) => new ContainerImage(repository, tag, i.ImageDigest, i.ImagePushedAt)
                )
                .First();
        }
        
        public async Task<IEnumerable<ContainerImage>> GetRepositoryTags(string repository)
        {
            using (_log.BeginScope(new Dictionary<string, object>()
            {
                {"Repository", repository}
            }))
            {
                _log.LogInformation("Getting repository tags");

                var repositories = await GetRepositoriesInternal();
                // var repositories = await _client.DescribeRepositoriesAsync(new DescribeRepositoriesRequest());
                var repo = repositories.First(r =>
                    r.repositoryUri.Equals(repository, StringComparison.OrdinalIgnoreCase));
                
                var describeImagesRequest = new DescribeImagesRequest()
                {
                    RegistryId = repo.registryId,
                    RepositoryName = repo.repositoryName,
                };
                
                var images = await _client.DescribeImagesAsync(describeImagesRequest, CancellationToken.None);

                _log.LogTrace("Building image list");
                var imageList = images.ImageDetails.SelectMany(
                        i => i.ImageTags,
                        (i, tag) => new ContainerImage(repository, tag, i.ImageDigest, i.ImagePushedAt)
                    )
                    .ToList();
                
                while (images.NextToken != null)
                {
                    describeImagesRequest.NextToken = images.NextToken;
                    images = await _client.DescribeImagesAsync(describeImagesRequest);

                    imageList.AddRange(
                        images.ImageDetails.SelectMany(
                            i => i.ImageTags,
                            (i, tag) => new ContainerImage(repository, tag, i.ImageDigest, i.ImagePushedAt)
                        )
                    );
                }
                
                _log.LogInformation("Found {ImageCount} images for {Repository}", imageList.Count, repository);

                return imageList;
            }
        }
    }
}