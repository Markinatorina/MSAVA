using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using M_SAVA_BLL.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace M_SAVA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RetrieveFileController : ControllerBase
    {
        private readonly ReturnFileService _returnFileService;

        public RetrieveFileController(ReturnFileService returnFileService)
        {
            _returnFileService = returnFileService ?? throw new ArgumentNullException(nameof(returnFileService));
        }

        [HttpGet("{id:guid}")]
        public IActionResult GetFileById(Guid id)
        {
            var result = _returnFileService.GetFileById(id);
            if (result == null)
                return NotFound();

            return result.FileStream;
        }

        [HttpGet("async/{id:guid}")]
        public async Task<IActionResult> GetFileByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _returnFileService.GetFileByIdAsync(id, cancellationToken);
            if (result == null)
                return NotFound();

            return result.FileStream;
        }
    }
}
