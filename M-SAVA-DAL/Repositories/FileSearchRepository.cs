using M_SAVA_DAL.Contexts;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories.Generic;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Repositories
{
    public class FileSearchRepository : IdentifiableRepository<SavedFileDataDB>
    {
        public FileSearchRepository(BaseDataContext context) : base(context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context), "Repository: BaseDataContext cannot be null.");
        }

        public async Task<List<SavedFileDataDB>> SearchFilesAsync(
            string? tag = null,
            string? category = null,
            string? name = null,
            string? description = null,
            CancellationToken cancellationToken = default)
        {
            var query = _entities.AsNoTracking().Include(f => f.FileReference)
                .Where(f =>
                    (string.IsNullOrWhiteSpace(tag) || (f.Tags != null && f.Tags.Any(t => t != null && t.ToLower() == tag.ToLower()))) &&
                    (string.IsNullOrWhiteSpace(category) || (f.Categories != null && f.Categories.Any(c => c != null && c.ToLower() == category.ToLower()))) &&
                    (string.IsNullOrWhiteSpace(name) || (f.Name != null && f.Name.ToLower().Contains(name.ToLower()))) &&
                    (string.IsNullOrWhiteSpace(description) || (f.Description != null && f.Description.ToLower().Contains(description.ToLower())))
                );
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<List<Guid>> SearchFileIdsAsync(
            string? tag = null,
            string? category = null,
            string? name = null,
            string? description = null,
            CancellationToken cancellationToken = default)
        {
            var query = _entities.AsNoTracking()
                .Where(f =>
                    (string.IsNullOrWhiteSpace(tag) || (f.Tags != null && f.Tags.Any(t => t != null && t.ToLower() == tag.ToLower()))) &&
                    (string.IsNullOrWhiteSpace(category) || (f.Categories != null && f.Categories.Any(c => c != null && c.ToLower() == category.ToLower()))) &&
                    (string.IsNullOrWhiteSpace(name) || (f.Name != null && f.Name.ToLower().Contains(name.ToLower()))) &&
                    (string.IsNullOrWhiteSpace(description) || (f.Description != null && f.Description.ToLower().Contains(description.ToLower())))
                );
            return await query.Select(f => f.Id).ToListAsync(cancellationToken);
        }
    }
}
