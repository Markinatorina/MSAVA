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
        private readonly IUserService _userService;

        public FileSearchService(BaseDataContext context, IUserService userService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        private static SearchFileDataDTO MapToDTO(SavedFileDataDB db)
        {
            return MappingUtils.MapSearchFileDataDTO(db);
        }

        private static IQueryable<SavedFileDataDB> ApplyAccessFilter(IQueryable<SavedFileDataDB> query, SessionDTO session)
        {
            var userId = session.UserId;
            var groupIds = session.AccessGroups;

            // Allow if:
            // - File is public (PublicViewing true)
            // - User is owner
            // - User is in the file reference access group
            return query.Where(f =>
                f.PublicViewing
                || f.OwnerId == userId
                || (f.FileReference != null && groupIds.Contains(f.FileReference.AccessGroupId))
            );
        }

        public async Task<List<Guid>> GetAllFileGuidsAsync(CancellationToken cancellationToken = default)
        {
            var session = _userService.GetSessionClaims();

            var baseQuery = _context.FileData
                .AsNoTracking()
                .AsQueryable();

            var filtered = ApplyAccessFilter(baseQuery, session);

            return await filtered
                .Select(f => f.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SearchFileDataDTO>> GetAllFileMetadataAsync(CancellationToken cancellationToken = default)
        {
            var session = _userService.GetSessionClaims();

            var baseQuery = _context.FileData
                .AsNoTracking()
                .AsQueryable();

            var filtered = ApplyAccessFilter(baseQuery, session)
                .Include(f => f.FileReference);

            var dbList = await filtered.ToListAsync(cancellationToken);
            return dbList.Select(MapToDTO).ToList();
        }

        public async Task<List<Guid>> GetFileGuidsByAllFieldsAsync(
            string? tag,
            string? category,
            string? name,
            string? description,
            CancellationToken cancellationToken = default)
        {
            var session = _userService.GetSessionClaims();

            var baseQuery = _context.FileData
                .AsNoTracking()
                .Where(f =>
                    (string.IsNullOrWhiteSpace(tag) || (f.Tags != null && f.Tags.Any(t => t != null && t.ToLower() == tag.ToLower()))) &&
                    (string.IsNullOrWhiteSpace(category) || (f.Categories != null && f.Categories.Any(c => c != null && c.ToLower() == category.ToLower()))) &&
                    (string.IsNullOrWhiteSpace(name) || (f.Name != null && f.Name.ToLower().Contains(name.ToLower()))) &&
                    (string.IsNullOrWhiteSpace(description) || (f.Description != null && f.Description.ToLower().Contains(description.ToLower())))
                );

            var filtered = ApplyAccessFilter(baseQuery, session);

            return await filtered.Select(f => f.Id).ToListAsync(cancellationToken);
        }

        public async Task<List<SearchFileDataDTO>> GetFileDataByAllFieldsAsync(
            string? tag,
            string? category,
            string? name,
            string? description,
            CancellationToken cancellationToken = default)
        {
            var session = _userService.GetSessionClaims();

            var baseQuery = _context.FileData
                .AsNoTracking()
                .Where(f =>
                    (string.IsNullOrWhiteSpace(tag) || (f.Tags != null && f.Tags.Any(t => t != null && t.ToLower() == tag.ToLower()))) &&
                    (string.IsNullOrWhiteSpace(category) || (f.Categories != null && f.Categories.Any(c => c != null && c.ToLower() == category.ToLower()))) &&
                    (string.IsNullOrWhiteSpace(name) || (f.Name != null && f.Name.ToLower().Contains(name.ToLower()))) &&
                    (string.IsNullOrWhiteSpace(description) || (f.Description != null && f.Description.ToLower().Contains(description.ToLower())))
                );

            var filtered = ApplyAccessFilter(baseQuery, session)
                .Include(f => f.FileReference);

            var dbList = await filtered.ToListAsync(cancellationToken);
            return dbList.Select(MapToDTO).ToList();
        }
    }
}
