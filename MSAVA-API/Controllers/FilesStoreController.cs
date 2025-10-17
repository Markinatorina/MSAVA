using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MSAVA_Shared.Models;
using MSAVA_BLL.Utils;
using MSAVA_DAL.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MSAVA_BLL.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace MSAVA_API.Controllers
{
    [Route("api/files/store")]
    [ApiController]
    [Authorize]
    public class FilesStoreController : ControllerBase
    {
        private readonly IStoreFileService _uploadService;

        public FilesStoreController(IStoreFileService saveFileService)
        {
            _uploadService = saveFileService ?? throw new ArgumentNullException(nameof(saveFileService));
        }

        [HttpPost("stream")]
        [Consumes("application/octet-stream")]
        public async Task<ActionResult<Guid>> CreateFileFromStream(
            [FromQuery] [Required] string fileName,
            [FromQuery] [Required] string fileExtension,
            [FromQuery] [Required] List<string> tags,
            [FromQuery] [Required] List<string> categories,
            [FromQuery] [Required] Guid accessGroup,
            [FromQuery] [Required] string description,
            [FromQuery] [Required] bool publicViewing,
            [FromQuery] [Required] bool publicDownload,
            CancellationToken cancellationToken = default)
        {
            SaveFileFromStreamDTO dto = new SaveFileFromStreamDTO
            {
                FileName = fileName,
                FileExtension = fileExtension,
                Tags = tags,
                Categories = categories,
                AccessGroupId = accessGroup,
                Description = description,
                PublicViewing = publicViewing,
                PublicDownload = publicDownload,
                Stream = Request.Body
            };

            Guid id = await _uploadService.CreateFileFromStreamAsync(dto, cancellationToken);
            return Ok(id);
        }

        [HttpPost("url")]
        public async Task<ActionResult<Guid>> CreateFileFromUrl(
            [FromForm][Required] SaveFileFromUrlDTO dto,
            CancellationToken cancellationToken = default)
        {
            var id = await _uploadService.CreateFileFromURLAsync(dto, cancellationToken);
            return Ok(id);
        }

        [HttpPost("formfile")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Guid>> CreateFileFromFormFile(
            [FromForm][Required] SaveFileFromFormFileDTO dto,
            CancellationToken cancellationToken = default)
        {
            var id = await _uploadService.CreateFileFromFormFileAsync(dto, cancellationToken);
            return Ok(id);
        }
    }
}
