using M_SAVA_Shared.Models;
using M_SAVA_BLL.Utils;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
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
        private readonly FileSearchRepository _fileSearchRepository;

        public FileSearchService(FileSearchRepository fileSearchRepository)
        {
            _fileSearchRepository = fileSearchRepository ?? throw new ArgumentNullException(nameof(fileSearchRepository));
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
            return await _fileSearchRepository.SearchFileIdsAsync(
                tag,
                category,
                name,
                description,
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
            var dbList = await _fileSearchRepository.SearchFilesAsync(
                tag,
                category,
                name,
                description,
                cancellationToken
            );
            return dbList.Select(MapToDTO).ToList();
        }
    }
}