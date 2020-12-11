using System;
using System.Collections.Generic;
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
        private readonly IEntityRepository<ContainerImageMetadata> _containerImageMetadata;
        private readonly IEntityRepository<ContainerImageRepository> _containerImageRepository;
        private readonly IEntityRepository<ContainerImageTag> _containerImageTagRepository;

        public ContainerImageMetadataService(
            IEntityRepository<ContainerImageMetadata> containerImageMetadata,
            IEntityRepository<ContainerImageRepository> containerImageRepository,
            IEntityRepository<ContainerImageTag> containerImageTagRepository
        )
        {
            _containerImageMetadata = containerImageMetadata;
            _containerImageRepository = containerImageRepository;
            _containerImageTagRepository = containerImageTagRepository;
        }

        private string NormalizeContainerRepository(string repository)
        {
            return repository.ToLower();
        }

        public async Task AddOrUpdate(ContainerImage containerImage)
        {
            var normalizedContainerRepository = NormalizeContainerRepository(containerImage.Repository);
            var repository = await _containerImageRepository
                                 .Query()
                                 .FirstOrDefaultAsync(
                                     x => x.Name == normalizedContainerRepository
                                 ) 
                             ??
                             await _containerImageRepository.Add(new ContainerImageRepository()
                             {
                                 Id = Guid.NewGuid(),
                                 Name = normalizedContainerRepository
                             });

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

            await _containerImageRepository.Save();
            await _containerImageMetadata.Save();
            await _containerImageTagRepository.Save();

        }

        public async Task<IEnumerable<ContainerImage>> GetTagsForRepository(string repository)
        {
            var normalizedContainerRepository = NormalizeContainerRepository(repository);

            var r = await _containerImageRepository.Query()
                .FirstOrDefaultAsync(x => x.Name.Equals(normalizedContainerRepository));

            if (r == null) return Enumerable.Empty<ContainerImage>();

            var containerImageTags = await _containerImageTagRepository.Query()
                .Where(x => x.RepositoryId == r.Id)
                .Include(x=>x.Metadata)
                .ToListAsync();
            
            if (containerImageTags == null) return Enumerable.Empty<ContainerImage>();

            return containerImageTags
                .Select(x =>
                    new ContainerImage(
                        r.Name, x.Tag, x.Metadata.Hash, 
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