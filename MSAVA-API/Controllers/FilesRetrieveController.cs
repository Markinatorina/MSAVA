using MSAVA_Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using MSAVA_BLL.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using MSAVA_API.Attributes;

namespace MSAVA_API.Controllers
{
    [Route("api/files/retrieve")]
    [ApiController]
    [Authorize]
    public class FilesRetrieveController : ControllerBase
    {
        private readonly IReturnFileService _returnFileService;
        private readonly ISearchFileService _searchFileService;

        public FilesRetrieveController(IReturnFileService returnFileService, ISearchFileService searchFileService)
        {
            _returnFileService = returnFileService ?? throw new ArgumentNullException(nameof(returnFileService));
            _searchFileService = searchFileService ?? throw new ArgumentNullException(nameof(searchFileService));
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

        [HttpGet("meta/all")]
        public async Task<ActionResult<List<SearchFileDataDTO>>> GetAllFileMetadata(CancellationToken cancellationToken)
        {
            var result = await _searchFileService.GetAllFileMetadataAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("meta/all/id")]
        public async Task<ActionResult<List<Guid>>> SearchFileGuidsByAllFields([FromQuery] string? tag, [FromQuery] string? category, [FromQuery] string? name, [FromQuery] string? description, CancellationToken cancellationToken)
        {
            var result = await _searchFileService.GetFileGuidsByAllFieldsAsync(tag, category, name, description, cancellationToken);
            return Ok(result);
        }

        [HttpGet("meta/all/data")]
        public async Task<ActionResult<List<SearchFileDataDTO>>> SearchFilesByAllFields([FromQuery] string? tag, [FromQuery] string? category, [FromQuery] string? name, [FromQuery] string? description, CancellationToken cancellationToken)
        {
            var result = await _searchFileService.GetFileDataByAllFieldsAsync(tag, category, name, description, cancellationToken);
            return Ok(result);
        }
    }
}
