using M_SAVA_Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Services.Interfaces
{
    public interface ISearchFileService
    {
        Task<List<Guid>> GetFileGuidsByTagAsync(string tag, CancellationToken cancellationToken = default);
        Task<List<Guid>> GetFileGuidsByCategoryAsync(string category, CancellationToken cancellationToken = default);
        Task<List<Guid>> GetFileGuidsByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<List<Guid>> GetFileGuidsByDescriptionAsync(string description, CancellationToken cancellationToken = default);
        Task<List<Guid>> GetFileGuidsByAllFieldsAsync(string? tag, string? category, string? name, string? description, CancellationToken cancellationToken = default);

        Task<List<SearchFileDataDTO>> GetFileDataByTagAsync(string tag, CancellationToken cancellationToken = default);
        Task<List<SearchFileDataDTO>> GetFileDataByCategoryAsync(string category, CancellationToken cancellationToken = default);
        Task<List<SearchFileDataDTO>> GetFileDataByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<List<SearchFileDataDTO>> GetFileDataByDescriptionAsync(string description, CancellationToken cancellationToken = default);
        Task<List<SearchFileDataDTO>> GetFileDataByAllFieldsAsync(string? tag, string? category, string? name, string? description, CancellationToken cancellationToken = default);
    }
}
