using M_SAVA_Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using M_SAVA_BLL.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using M_SAVA_API.Attributes;

namespace M_SAVA_API.Controllers
{
    [Route("api/files/retrieve")]
    [ApiController]
    [Authorize]
    public class FilesRetrieveController : ControllerBase
    {
        private readonly IReturnFileService _returnFileService;
        private readonly IWebHostEnvironment _env;

        public FilesRetrieveController(IReturnFileService returnFileService, IWebHostEnvironment env)
        {
            _returnFileService = returnFileService ?? throw new ArgumentNullException(nameof(returnFileService));
            _env = env;
        }

        [HttpGet("stream/{refId:guid}")]
        public IActionResult GetFileStreamById(Guid refId)
        {
            var dto = _returnFileService.GetFileStreamById(refId);
            return new FileStreamResult(dto.FileStream, "application/octet-stream")
            {
                FileDownloadName = $"{dto.FileName}{dto.FileExtension}"
            };
        }

        [HttpGet("stream/{**fileNameWithExtension}")]
        public IActionResult GetFileStreamByPath([TaintedPathCheck]string fileNameWithExtension)
        {
            var dto = _returnFileService.GetFileStreamByPath(fileNameWithExtension);
            return new FileStreamResult(dto.FileStream, "application/octet-stream")
            {
                FileDownloadName = $"{dto.FileName}{dto.FileExtension}"
            };
        }

        [HttpGet("physical/{refId:guid}")]
        public IActionResult GetPhysicalFileReturnDataById(Guid refId)
        {
            PhysicalReturnFileDTO fileData = _returnFileService.GetPhysicalFileReturnDataById(refId);

            return PhysicalFile(fileData.FilePath, fileData.ContentType, fileData.FileName, enableRangeProcessing: true);
        }

        [HttpGet("physical/{**fileNameWithExtension}")]
        public IActionResult GetPhysicalFileByPath([TaintedPathCheck]string fileNameWithExtension)
        {
            PhysicalReturnFileDTO fileData = _returnFileService.GetPhysicalFileReturnDataByPath(fileNameWithExtension);

            return PhysicalFile(fileData.FilePath, fileData.ContentType, fileData.FileName, enableRangeProcessing: true);
        }
    }
}
