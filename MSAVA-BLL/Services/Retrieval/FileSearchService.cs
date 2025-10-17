using MSAVA_Shared.Models;
using MSAVA_BLL.Utils;
using MSAVA_DAL.Models;
using MSAVA_DAL.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MSAVA_BLL.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MSAVA_BLL.Services.Retrieval
{
    public class FileSearchService : ISearchFileService
    {
        private readonly BaseDataContext _context;

        public FileSearchService(BaseDataContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private static SearchFileDataDTO MapToDTO(SavedFileDataDB db)
        {
            return MappingUtils.MapSearchFileDataDTO(db);
        }

        public async Task<List<Guid>> GetFileGuidsByAllFieldsAsync(
            string? tag,
            string? category,
            string? name,
            string? description,
            CancellationToken cancellationToken = default)
        {
            var query = _context.FileData
                .AsNoTracking()
                .Where(f =>
                    (string.IsNullOrWhiteSpace(tag) || (f.Tags != null && f.Tags.Any(t => t != null && t.ToLower() == tag.ToLower()))) &&
                    (string.IsNullOrWhiteSpace(category) || (f.Categories != null && f.Categories.Any(c => c != null && c.ToLower() == category.ToLower()))) &&
                    (string.IsNullOrWhiteSpace(name) || (f.Name != null && f.Name.ToLower().Contains(name.ToLower()))) &&
                    (string.IsNullOrWhiteSpace(description) || (f.Description != null && f.Description.ToLower().Contains(description.ToLower())))
                );

            return await query.Select(f => f.Id).ToListAsync(cancellationToken);
        }

        public async Task<List<SearchFileDataDTO>> GetFileDataByAllFieldsAsync(
            string? tag,
            string? category,
            string? name,
            string? description,
            CancellationToken cancellationToken = default)
        {
            var query = _context.FileData
                .AsNoTracking()
                .Include(f => f.FileReference)
                .Where(f =>
                    (string.IsNullOrWhiteSpace(tag) || (f.Tags != null && f.Tags.Any(t => t != null && t.ToLower() == tag.ToLower()))) &&
                    (string.IsNullOrWhiteSpace(category) || (f.Categories != null && f.Categories.Any(c => c != null && c.ToLower() == category.ToLower()))) &&
                    (string.IsNullOrWhiteSpace(name) || (f.Name != null && f.Name.ToLower().Contains(name.ToLower()))) &&
                    (string.IsNullOrWhiteSpace(description) || (f.Description != null && f.Description.ToLower().Contains(description.ToLower())))
                );

            var dbList = await query.ToListAsync(cancellationToken);
            return dbList.Select(MapToDTO).ToList();
        }
    }
}