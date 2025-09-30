using M_SAVA_Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Services.Interfaces
{
    public interface ISearchFileService
    {
        Task<List<Guid>> GetFileGuidsByAllFieldsAsync(string? tag, string? category, string? name, string? description, CancellationToken cancellationToken = default);
        Task<List<SearchFileDataDTO>> GetFileDataByAllFieldsAsync(string? tag, string? category, string? name, string? description, CancellationToken cancellationToken = default);
    }
}
