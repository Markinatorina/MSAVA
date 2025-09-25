using M_SAVA_Shared.Models;
using M_SAVA_BLL.Utils;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using M_SAVA_BLL.Services.Interfaces;

namespace M_SAVA_BLL.Services.Retrieval
{
    public class FileSearchService : ISearchFileService
    {
        private readonly IIdentifiableRepository<SavedFileDataDB> _dataRepository;

        public FileSearchService(IIdentifiableRepository<SavedFileDataDB> dataRepository)
        {
            _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository), "Service: dataRepository cannot be null.");
        }

        // Helper to always include FileReference
        private IQueryable<SavedFileDataDB> GetAllWithFileReference()
        {
            return _dataRepository.GetAll().Include(f => f.FileReference).AsNoTracking();
        }

        public List<Guid> GetFileGuidsByTag(string tag)
        {
            string loweredTag = tag.ToLower();
            return _dataRepository.GetAllAsReadOnly()
                .Where(f => f.Tags != null && f.Tags.Any(t => t != null && t.ToLower() == loweredTag))
                .Select(f => f.Id)
                .ToList();
        }

        public List<Guid> GetFileGuidsByCategory(string category)
        {
            string loweredCategory = category.ToLower();
            return _dataRepository.GetAllAsReadOnly()
                .Where(f => f.Categories != null && f.Categories.Any(c => c != null && c.ToLower() == loweredCategory))
                .Select(f => f.Id)
                .ToList();
        }

        public List<Guid> GetFileGuidsByName(string name)
        {
            string loweredName = name.ToLower();
            return _dataRepository.GetAllAsReadOnly()
                .Where(f => f.Name != null && f.Name.ToLower().Contains(loweredName))
                .Select(f => f.Id)
                .ToList();
        }

        public List<Guid> GetFileGuidsByDescription(string description)
        {
            string loweredDescription = description.ToLower();
            return _dataRepository.GetAllAsReadOnly()
                .Where(f => f.Description != null && f.Description.ToLower().Contains(loweredDescription))
                .Select(f => f.Id)
                .ToList();
        }

        public List<Guid> GetFileGuidsByAllFields(string? tag, string? category, string? name, string? description)
        {
            var query = _dataRepository.GetAllAsReadOnly();

            if (!string.IsNullOrWhiteSpace(tag))
            {
                string loweredTag = tag.ToLower();
                query = query.Where(f => f.Tags != null && f.Tags.Any(t => t != null && t.ToLower() == loweredTag));
            }
            if (!string.IsNullOrWhiteSpace(category))
            {
                string loweredCategory = category.ToLower();
                query = query.Where(f => f.Categories != null && f.Categories.Any(c => c != null && c.ToLower() == loweredCategory));
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                string loweredName = name.ToLower();
                query = query.Where(f => f.Name != null && f.Name.ToLower().Contains(loweredName));
            }
            if (!string.IsNullOrWhiteSpace(description))
            {
                string loweredDescription = description.ToLower();
                query = query.Where(f => f.Description != null && f.Description.ToLower().Contains(loweredDescription));
            }

            return query.Select(f => f.Id).ToList();
        }

        public List<SearchFileDataDTO> GetFileDataByTag(string tag)
        {
            string loweredTag = tag.ToLower();
            return _dataRepository.GetAll().Include(f => f.FileReference).AsNoTracking()
                .Where(f => f.Tags != null && f.Tags.Any(t => t != null && t.ToLower() == loweredTag))
                .Select(MappingUtils.MapSearchFileDataDTO)
                .ToList();
        }

        public List<SearchFileDataDTO> GetFileDataByCategory(string category)
        {
            string loweredCategory = category.ToLower();
            return _dataRepository.GetAll().Include(f => f.FileReference).AsNoTracking()
                .Where(f => f.Categories != null && f.Categories.Any(c => c != null && c.ToLower() == loweredCategory))
                .Select(MappingUtils.MapSearchFileDataDTO)
                .ToList();
        }

        public List<SearchFileDataDTO> GetFileDataByName(string name)
        {
            string loweredName = name.ToLower();
            return _dataRepository.GetAll().Include(f => f.FileReference).AsNoTracking()
                .Where(f => f.Name != null && f.Name.ToLower().Contains(loweredName))
                .Select(MappingUtils.MapSearchFileDataDTO)
                .ToList();
        }

        public List<SearchFileDataDTO> GetFileDataByDescription(string description)
        {
            string loweredDescription = description.ToLower();
            return _dataRepository.GetAll().Include(f => f.FileReference).AsNoTracking()
                .Where(f => f.Description != null && f.Description.ToLower().Contains(loweredDescription))
                .Select(MappingUtils.MapSearchFileDataDTO)
                .ToList();
        }

        public List<SearchFileDataDTO> GetFileDataByAllFields(string? tag, string? category, string? name, string? description)
        {
            var query = _dataRepository.GetAll().Include(f => f.FileReference).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(tag))
            {
                string loweredTag = tag.ToLower();
                query = query.Where(f => f.Tags != null && f.Tags.Any(t => t != null && t.ToLower() == loweredTag));
            }
            if (!string.IsNullOrWhiteSpace(category))
            {
                string loweredCategory = category.ToLower();
                query = query.Where(f => f.Categories != null && f.Categories.Any(c => c != null && c.ToLower() == loweredCategory));
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                string loweredName = name.ToLower();
                query = query.Where(f => f.Name != null && f.Name.ToLower().Contains(loweredName));
            }
            if (!string.IsNullOrWhiteSpace(description))
            {
                string loweredDescription = description.ToLower();
                query = query.Where(f => f.Description != null && f.Description.ToLower().Contains(loweredDescription));
            }

            return query.Select(MappingUtils.MapSearchFileDataDTO).ToList();
        }
    }
}