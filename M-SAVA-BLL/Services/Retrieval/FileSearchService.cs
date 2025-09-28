using M_SAVA_Shared.Models;
using M_SAVA_BLL.Utils;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using M_SAVA_BLL.Services.Interfaces;

namespace M_SAVA_BLL.Services.Retrieval
{
    public class FileSearchService : ISearchFileService
    {
        private readonly IIdentifiableRepository<SavedFileDataDB> _dataRepository;

        // DTO selector
        private static readonly Expression<Func<SavedFileDataDB, SearchFileDataDTO>> SearchFileDataSelector = f =>
            new SearchFileDataDTO
            {
                DataId = f.Id,
                RefId = f.FileReferenceId,
                FilePath = f.FileReference != null ? f.FileReference.FileExtension.ToString() : string.Empty,
                Name = f.Name,
                Description = f.Description,
                MimeType = f.MimeType,
                FileExtension = f.FileExtension,
                Tags = f.Tags,
                Categories = f.Categories,
                SizeInBytes = f.SizeInBytes,
                Checksum = f.Checksum,
                Metadata = f.Metadata,
                PublicViewing = f.PublicViewing,
                DownloadCount = f.DownloadCount,
                SavedAt = f.SavedAt,
                LastModifiedAt = f.LastModifiedAt,
                OwnerId = f.OwnerId,
                LastModifiedById = f.LastModifiedById
            };

        // filters
        private static Expression<Func<SavedFileDataDB, bool>> TagFilter(string tag) =>
            f => f.Tags != null && f.Tags.Any(t => t != null && t.ToLower() == tag.ToLower());

        private static Expression<Func<SavedFileDataDB, bool>> CategoryFilter(string category) =>
            f => f.Categories != null && f.Categories.Any(c => c != null && c.ToLower() == category.ToLower());

        private static Expression<Func<SavedFileDataDB, bool>> NameFilter(string name) =>
            f => f.Name != null && f.Name.ToLower().Contains(name.ToLower());

        private static Expression<Func<SavedFileDataDB, bool>> DescriptionFilter(string description) =>
            f => f.Description != null && f.Description.ToLower().Contains(description.ToLower());

        public FileSearchService(IIdentifiableRepository<SavedFileDataDB> dataRepository)
        {
            _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository), "Service: dataRepository cannot be null.");
        }

        public async Task<List<Guid>> GetFileGuidsByTagAsync(
            string tag,
            CancellationToken cancellationToken = default)
        {
            return await _dataRepository.GetFilteredAsync(
                f => f.Id,
                q => q.Where(TagFilter(tag)),
                cancellationToken
            );
        }

        public async Task<List<Guid>> GetFileGuidsByCategoryAsync(
            string category,
            CancellationToken cancellationToken = default)
        {
            return await _dataRepository.GetFilteredAsync(
                f => f.Id,
                q => q.Where(CategoryFilter(category)),
                cancellationToken
            );
        }

        public async Task<List<Guid>> GetFileGuidsByNameAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            return await _dataRepository.GetFilteredAsync(
                f => f.Id,
                q => q.Where(NameFilter(name)),
                cancellationToken
            );
        }

        public async Task<List<Guid>> GetFileGuidsByDescriptionAsync(
            string description,
            CancellationToken cancellationToken = default)
        {
            return await _dataRepository.GetFilteredAsync(
                f => f.Id,
                q => q.Where(DescriptionFilter(description)),
                cancellationToken
            );
        }

        public async Task<List<Guid>> GetFileGuidsByAllFieldsAsync(
            string? tag,
            string? category,
            string? name,
            string? description,
            CancellationToken cancellationToken = default)
        {
            Func<IQueryable<SavedFileDataDB>, IQueryable<SavedFileDataDB>> queryShaper = q =>
            {
                if (!string.IsNullOrWhiteSpace(tag))
                    q = q.Where(TagFilter(tag));
                if (!string.IsNullOrWhiteSpace(category))
                    q = q.Where(CategoryFilter(category));
                if (!string.IsNullOrWhiteSpace(name))
                    q = q.Where(NameFilter(name));
                if (!string.IsNullOrWhiteSpace(description))
                    q = q.Where(DescriptionFilter(description));
                return q;
            };
            return await _dataRepository.GetFilteredAsync(
                f => f.Id,
                queryShaper,
                cancellationToken
            );
        }

        public async Task<List<SearchFileDataDTO>> GetFileDataByTagAsync(
            string tag,
            CancellationToken cancellationToken = default)
        {
            return await _dataRepository.GetFilteredAsync(
                SearchFileDataSelector,
                q => q.Include(f => f.FileReference).Where(TagFilter(tag)),
                cancellationToken
            );
        }

        public async Task<List<SearchFileDataDTO>> GetFileDataByCategoryAsync(
            string category,
            CancellationToken cancellationToken = default)
        {
            return await _dataRepository.GetFilteredAsync(
                SearchFileDataSelector,
                q => q.Include(f => f.FileReference).Where(CategoryFilter(category)),
                cancellationToken
            );
        }

        public async Task<List<SearchFileDataDTO>> GetFileDataByNameAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            return await _dataRepository.GetFilteredAsync(
                SearchFileDataSelector,
                q => q.Include(f => f.FileReference).Where(NameFilter(name)),
                cancellationToken
            );
        }

        public async Task<List<SearchFileDataDTO>> GetFileDataByDescriptionAsync(
            string description,
            CancellationToken cancellationToken = default)
        {
            return await _dataRepository.GetFilteredAsync(
                SearchFileDataSelector,
                q => q.Include(f => f.FileReference).Where(DescriptionFilter(description)),
                cancellationToken
            );
        }

        public async Task<List<SearchFileDataDTO>> GetFileDataByAllFieldsAsync(
            string? tag,
            string? category,
            string? name,
            string? description,
            CancellationToken cancellationToken = default)
        {
            Func<IQueryable<SavedFileDataDB>, IQueryable<SavedFileDataDB>> queryShaper = q =>
            {
                q = q.Include(f => f.FileReference);
                if (!string.IsNullOrWhiteSpace(tag))
                    q = q.Where(TagFilter(tag));
                if (!string.IsNullOrWhiteSpace(category))
                    q = q.Where(CategoryFilter(category));
                if (!string.IsNullOrWhiteSpace(name))
                    q = q.Where(NameFilter(name));
                if (!string.IsNullOrWhiteSpace(description))
                    q = q.Where(DescriptionFilter(description));
                return q;
            };
            return await _dataRepository.GetFilteredAsync(
                SearchFileDataSelector,
                queryShaper,
                cancellationToken
            );
        }
    }
}