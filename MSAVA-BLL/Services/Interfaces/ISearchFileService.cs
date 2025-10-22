using MSAVA_Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MSAVA_BLL.Services.Interfaces
{
    public interface ISearchFileService
    {
        Task<List<Guid>> GetFileGuidsByAllFieldsAsync(string? tag, string? category, string? name, string? description, CancellationToken cancellationToken = default);
        Task<List<SearchFileDataDTO>> GetFileDataByAllFieldsAsync(string? tag, string? category, string? name, string? description, CancellationToken cancellationToken = default);
        Task<List<Guid>> GetAllFileGuidsAsync(CancellationToken cancellationToken = default);
        Task<List<SearchFileDataDTO>> GetAllFileMetadataAsync(CancellationToken cancellationToken = default);
    }
}
