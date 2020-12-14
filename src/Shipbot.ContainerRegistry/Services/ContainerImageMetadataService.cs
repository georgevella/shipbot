using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Schema;
using Microsoft.EntityFrameworkCore;
using Shipbot.ContainerRegistry.Dao;
using Shipbot.ContainerRegistry.Models;
using Shipbot.Data;

namespace Shipbot.ContainerRegistry.Services
{
    public class ContainerImageMetadataService : IContainerImageMetadataService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRegistryClientPool _registryClientPool;
        private readonly IEntityRepository<ContainerImageMetadata> _containerImageMetadata;
        private readonly IEntityRepository<ContainerImageRepository> _containerImageRepository;
        private readonly IEntityRepository<ContainerImageTag> _containerImageTagRepository;

        private static readonly IReadOnlyCollection<ContainerImage> EmptyContainerImageCollection =
            new ReadOnlyCollection<ContainerImage>(Enumerable.Empty<ContainerImage>().ToList());

        public ContainerImageMetadataService(
            IUnitOfWork unitOfWork,
            IRegistryClientPool registryClientPool,
            IEntityRepository<ContainerImageMetadata> containerImageMetadata,
            IEntityRepository<ContainerImageRepository> containerImageRepository,
            IEntityRepository<ContainerImageTag> containerImageTagRepository
        )
        {
            _unitOfWork = unitOfWork;
            _registryClientPool = registryClientPool;
            _containerImageMetadata = containerImageMetadata;
            _containerImageRepository = containerImageRepository;
            _containerImageTagRepository = containerImageTagRepository;
        }

        private string NormalizeContainerRepository(string repository)
        {
            return repository.ToLower();
        }

        public async Task AddOrUpdate(IEnumerable<ContainerImage> containerImages)
        {
            if (containerImages == null) throw new ArgumentNullException(nameof(containerImages));
            if (containerImages.Count() == 0)
                throw new ArgumentOutOfRangeException(nameof(containerImages), "Empty container image list");
            
            foreach (var containerImage in containerImages)
            {
                await AddOrUpdateWorker(containerImage);
            }
            
            await _unitOfWork.Commit();
        }

        public async Task AddOrUpdate(ContainerImage containerImage)
        {
            if (containerImage == null) 
                throw new ArgumentNullException(nameof(containerImage));
            
            await AddOrUpdateWorker(containerImage);
            await _unitOfWork.Commit();
        }

        private async Task AddOrUpdateWorker(ContainerImage containerImage)
        {
            var normalizedContainerRepository = NormalizeContainerRepository(containerImage.Repository);
            var repository = await GetRepositoryDao(normalizedContainerRepository);

            var metadata = await _containerImageMetadata
                               .Query()
                               .FirstOrDefaultAsync(
                                   x =>
                                       x.RepositoryId == repository.Id &&
                                       x.Hash == containerImage.Hash
                               )
                           ??
                           await _containerImageMetadata.Add(new ContainerImageMetadata()
                           {
                               Hash = containerImage.Hash,
                               Id = Guid.NewGuid(),
                               Repository = repository,
                               CreatedDateTime = containerImage.CreationDateTime.UtcDateTime
                           });

            var tag = await _containerImageTagRepository.Query()
                .FirstOrDefaultAsync(x => x.RepositoryId == repository.Id && x.Tag == containerImage.Tag);

            if (tag == null)
            {
                // create a new tag entry
                tag = await _containerImageTagRepository.Add(new ContainerImageTag()
                {
                    Id = Guid.NewGuid(),
                    Metadata = metadata,
                    Repository = repository,
                    Tag = containerImage.Tag
                });
            }
            else
            {
                // check if we are updating the tag
                if (tag.MetadataId != metadata.Id)
                {
                    // tag had it's hash changed
                    tag.Metadata = metadata;
                }
            }
        }

        private async Task<ContainerImageRepository> GetRepositoryDao(string normalizedContainerRepository)
        {
            var repository = await _containerImageRepository
                .Query()
                .FirstOrDefaultAsync(
                    x => x.Name == normalizedContainerRepository
                );

            if (repository == null)
            {
                repository = await _containerImageRepository.Add(new ContainerImageRepository()
                {
                    Id = Guid.NewGuid(),
                    Name = normalizedContainerRepository
                });

                await _containerImageRepository.Save();
            } 
                             
            return repository;
        }

        public async Task<IReadOnlyCollection<ContainerImage>> GetTagsForRepository(string repository)
        {
            var normalizedContainerRepository = NormalizeContainerRepository(repository);

            // var r = await _containerImageRepository.Query()
            //     .FirstOrDefaultAsync(x => x.Name.Equals(normalizedContainerRepository));
            //
            // if (r == null) return EmptyContainerImageCollection;

            var containerImageTags = await _containerImageTagRepository.Query()
                .Where(x => x.Repository.Name == normalizedContainerRepository)
                .Include(x=>x.Repository)
                .Include(x=>x.Metadata)
                .ToListAsync();
            
            if (containerImageTags == null) return EmptyContainerImageCollection;

            return containerImageTags
                .Select(x =>
                    new ContainerImage(
                        x.Repository.Name, x.Tag, x.Metadata.Hash, 
                        DateTime.SpecifyKind(x.Metadata.CreatedDateTime, DateTimeKind.Utc)
                    )
                )
                .ToList();
        }

        public async Task<ContainerImage> GetContainerImageByTag(string repository, string tag)
        {
            var normalizedContainerRepository = NormalizeContainerRepository(repository);

            var r = await _containerImageRepository.Query()
                .FirstOrDefaultAsync(x => x.Name.Equals(normalizedContainerRepository));
            
            var containerImageTags = await _containerImageTagRepository.Query()
                .Where(x => x.RepositoryId == r.Id && x.Tag == tag)
                .Include(x=>x.Metadata)
                .ToListAsync();

            if (containerImageTags.Any())
            {
                return containerImageTags
                    .Select(
                        x =>
                            new ContainerImage(
                                r.Name, x.Tag, x.Metadata.Hash,
                                DateTime.SpecifyKind(x.Metadata.CreatedDateTime, DateTimeKind.Utc)
                            )
                    )
                    .First();
            }
            
            // we did not find the tag into the local cache, try to get the tag information from remote
            var client = await _registryClientPool.GetRegistryClientForRepository(repository);
            var remoteContainerRepositoryTags = await client.GetRepositoryTags(repository);

            var remoteContainerImage = remoteContainerRepositoryTags.FirstOrDefault(x => x.Tag == tag);
            if (remoteContainerImage != null)
            {
                await AddOrUpdate(remoteContainerImage);
                return remoteContainerImage;
            }

            throw new InvalidOperationException("Could not find container image tag");
        }

        public async Task<(bool success, ContainerImage image)> TryGetContainerImageByTag(string repository, string tag)
        {
            try
            {
                var image = await GetContainerImageByTag(repository, tag);
                return (true, image);
            }
            catch
            {
                return (false, ContainerImage.Empty);
            }
        }
    }
}