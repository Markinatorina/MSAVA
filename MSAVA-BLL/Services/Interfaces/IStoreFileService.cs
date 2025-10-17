using MSAVA_Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSAVA_BLL.Services.Interfaces
{
    public interface IStoreFileService
    {
        Task<Guid> CreateFileFromStreamAsync(SaveFileFromStreamDTO dto, CancellationToken cancellationToken = default);
        Task<Guid> CreateFileFromURLAsync(
            SaveFileFromUrlDTO dto,
            CancellationToken cancellationToken = default);
        Task<Guid> CreateFileFromFormFileAsync(
            SaveFileFromFormFileDTO dto,
            CancellationToken cancellationToken = default);
    }
}