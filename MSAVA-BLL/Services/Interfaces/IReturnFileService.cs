using MSAVA_Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MSAVA_BLL.Services.Interfaces
{
    public interface IReturnFileService
    {
        StreamReturnFileDTO GetFileStreamById(Guid id);
        StreamReturnFileDTO GetFileStreamByPath(string fileNameWithExtension);
        PhysicalReturnFileDTO GetPhysicalFileReturnDataById(Guid id);
        PhysicalReturnFileDTO GetPhysicalFileReturnDataByPath(string path);
    }
}
