using MSAVA_Shared.Models;
using MSAVA_DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using MSAVA_BLL.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace MSAVA_API.Controllers
{
    [Route("api/files/search")]
    [ApiController]
    [Authorize]
    public class FilesSearchController : ControllerBase
    {
        private readonly ISearchFileService _searchFileService;

        public FilesSearchController(ISearchFileService searchFileService)
        {
            _searchFileService = searchFileService ?? throw new ArgumentNullException(nameof(searchFileService));
        }

        [HttpGet("byAll/id")]
        public async Task<ActionResult<List<Guid>>> SearchFileGuidsByAllFields([FromQuery] string? tag, [FromQuery] string? category, [FromQuery] string? name, [FromQuery] string? description, CancellationToken cancellationToken)
        {
            var result = await _searchFileService.GetFileGuidsByAllFieldsAsync(tag, category, name, description, cancellationToken);
            return Ok(result);
        }

        [HttpGet("byAll/data")]
        public async Task<ActionResult<List<SearchFileDataDTO>>> SearchFilesByAllFields([FromQuery] string? tag, [FromQuery] string? category, [FromQuery] string? name, [FromQuery] string? description, CancellationToken cancellationToken)
        {
            var result = await _searchFileService.GetFileDataByAllFieldsAsync(tag, category, name, description, cancellationToken);
            return Ok(result);
        }
    }
}
