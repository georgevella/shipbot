using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.ECR;
using Amazon.ECR.Model;
using Microsoft.Extensions.Logging;

namespace Shipbot.Controller.Core.ContainerRegistry.Ecr
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
            var repositories = await _client.DescribeRepositoriesAsync(new DescribeRepositoriesRequest());

            return repositories.Repositories.Any(r =>
                r.RepositoryUri.Equals(repository, StringComparison.OrdinalIgnoreCase)
            );
        }

        public async Task<List<(string tag, DateTime createdAt)>> GetRepositoryTags(string repository)
        {
            using (_log.BeginScope(new Dictionary<string, object>()
            {
                {"Repository", repository}
            }))
            {
                _log.LogTrace("Getting tags for {Repository}", repository);
                
                var repositories = await _client.DescribeRepositoriesAsync(new DescribeRepositoriesRequest());
                var repo = repositories.Repositories.First(r =>
                    r.RepositoryUri.Equals(repository, StringComparison.OrdinalIgnoreCase));
                
                var describeImagesRequest = new DescribeImagesRequest()
                {
                    RegistryId = repo.RegistryId,
                    RepositoryName = repo.RepositoryName,
                };
                
                var images = await _client.DescribeImagesAsync(describeImagesRequest, CancellationToken.None);

                _log.LogTrace("Building image list");
                var imageList = images.ImageDetails.SelectMany(
                        i => i.ImageTags,
                        (i, tag) => (tag, i.ImagePushedAt)
                    )
                    .ToList();
                
                while (images.NextToken != null)
                {
                    describeImagesRequest.NextToken = images.NextToken;
                    images = await _client.DescribeImagesAsync(describeImagesRequest);

                    imageList.AddRange(
                        images.ImageDetails.SelectMany(
                            i => i.ImageTags,
                            (i, tag) => (tag, i.ImagePushedAt)
                        )
                    );
                }
                
                _log.LogTrace("Found {ImageCount} images for {Repository}", imageList.Count, repository);

                return imageList;
            }
        }
    }
}