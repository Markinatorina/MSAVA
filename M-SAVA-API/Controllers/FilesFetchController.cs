using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using M_SAVA_Shared.Models;
using System.ComponentModel.DataAnnotations;
using M_SAVA_BLL.Services.Fetch;

namespace M_SAVA_API.Controllers
{
    [Route("api/files/fetch")]
    [ApiController]
    [Authorize]
    public class FilesFetchController : ControllerBase
    {
        private readonly FetchYouTubeFileService _fetchYouTubeFileService;
        private readonly FetchGoogleDriveService _fetchGoogleDriveFileService;
        private readonly FetchOneDriveService _fetchOneDriveFileService;

        public FilesFetchController(
            FetchYouTubeFileService fetchYouTubeFileService,
            FetchGoogleDriveService fetchGoogleDriveFileService,
            FetchOneDriveService fetchOneDriveFileService
        )
        {
            _fetchYouTubeFileService = fetchYouTubeFileService ?? throw new ArgumentNullException(nameof(fetchYouTubeFileService));
            _fetchGoogleDriveFileService = fetchGoogleDriveFileService ?? throw new ArgumentNullException(nameof(fetchGoogleDriveFileService));
            _fetchOneDriveFileService = fetchOneDriveFileService ?? throw new ArgumentNullException(nameof(fetchOneDriveFileService));
        }

        [HttpPost("youtube/noauth")]
        public async Task<ActionResult<Guid>> CreateFileFromYouTube(
            [FromBody][Required] FetchFileYouTubeDTO dto,
            CancellationToken cancellationToken = default)
        {
            var id = await _fetchYouTubeFileService.NoAuthFileFetch(dto, cancellationToken);
            return Ok(id);
        }

        [HttpPost("googledrive/noauth")]
        public async Task<ActionResult<Guid>> CreateFileFromGoogleDrive(
            [FromBody][Required] FetchFileGoogleDriveDTO dto,
            CancellationToken cancellationToken = default)
        {
            var id = await _fetchGoogleDriveFileService.NoAuthFileFetch(dto, cancellationToken);
            return Ok(id);
        }

        [HttpPost("onedrive/noauth")]
        public async Task<ActionResult<Guid>> CreateFileFromOneDrive(
            [FromBody][Required] FetchFileFromOneDriveDTO dto,
            CancellationToken cancellationToken = default)
        {
            var id = await _fetchOneDriveFileService.NoAuthFileFetch(dto, cancellationToken);
            return Ok(id);
        }
    }
}
