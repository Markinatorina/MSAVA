using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using M_SAVA_Core.Models;
using M_SAVA_BLL.Services;
using System.ComponentModel.DataAnnotations;

namespace M_SAVA_API.Controllers
{
    [Route("api/files/fetch")]
    [ApiController]
    [Authorize]
    public class FilesFetchController : ControllerBase
    {
        private readonly FetchFileService _fetchFileService;

        public FilesFetchController(FetchFileService fetchFileService)
        {
            _fetchFileService = fetchFileService ?? throw new ArgumentNullException(nameof(fetchFileService));
        }

        [HttpPost("youtube")]
        public async Task<ActionResult<Guid>> CreateFileFromYouTube(
            [FromBody][Required] SaveFileFromYouTubeDTO dto,
            CancellationToken cancellationToken = default)
        {
            var id = await _fetchFileService.CreateFileFromYouTubeAsync(dto, cancellationToken);
            return Ok(id);
        }
    }
}
