using M_SAVA_Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Services.Interfaces
{
    public interface ISearchFileService
    {
        List<Guid> GetFileGuidsByTag(string tag);
        List<Guid> GetFileGuidsByCategory(string category);
        List<Guid> GetFileGuidsByName(string name);
        List<Guid> GetFileGuidsByDescription(string description);
        List<Guid> GetFileGuidsByAllFields(string? tag, string? category, string? name, string? description);

        List<SearchFileDataDTO> GetFileDataByTag(string tag);
        List<SearchFileDataDTO> GetFileDataByCategory(string category);
        List<SearchFileDataDTO> GetFileDataByName(string name);
        List<SearchFileDataDTO> GetFileDataByDescription(string description);
        List<SearchFileDataDTO> GetFileDataByAllFields(string? tag, string? category, string? name, string? description);
    }
}
