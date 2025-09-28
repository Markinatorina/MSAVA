using M_SAVA_Shared.Models;
using M_SAVA_DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using M_SAVA_BLL.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace M_SAVA_API.Controllers
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

        [HttpGet("byTag/id")]
        public async Task<ActionResult<List<Guid>>> SearchFileGuidsByTag([FromQuery][Required] string tag, CancellationToken cancellationToken)
        {
            var result = await _searchFileService.GetFileGuidsByTagAsync(tag, cancellationToken);
            return Ok(result);
        }

        [HttpGet("byCategory/id")]
        public async Task<ActionResult<List<Guid>>> SearchFileGuidsByCategory([FromQuery][Required] string category, CancellationToken cancellationToken)
        {
            var result = await _searchFileService.GetFileGuidsByCategoryAsync(category, cancellationToken);
            return Ok(result);
        }

        [HttpGet("byName/id")]
        public async Task<ActionResult<List<Guid>>> SearchFileGuidsByName([FromQuery][Required] string name, CancellationToken cancellationToken)
        {
            var result = await _searchFileService.GetFileGuidsByNameAsync(name, cancellationToken);
            return Ok(result);
        }

        [HttpGet("byDescription/id")]
        public async Task<ActionResult<List<Guid>>> SearchFileGuidsByDescription([FromQuery][Required] string description, CancellationToken cancellationToken)
        {
            var result = await _searchFileService.GetFileGuidsByDescriptionAsync(description, cancellationToken);
            return Ok(result);
        }

        [HttpGet("byAll/data")]
        public async Task<ActionResult<List<SearchFileDataDTO>>> SearchFilesByAllFields([FromQuery] string? tag, [FromQuery] string? category, [FromQuery] string? name, [FromQuery] string? description, CancellationToken cancellationToken)
        {
            var result = await _searchFileService.GetFileDataByAllFieldsAsync(tag, category, name, description, cancellationToken);
            return Ok(result);
        }

        [HttpGet("byTag/data")]
        public async Task<ActionResult<List<SearchFileDataDTO>>> SearchFilesByTag([FromQuery][Required] string tag, CancellationToken cancellationToken)
        {
            var result = await _searchFileService.GetFileDataByTagAsync(tag, cancellationToken);
            return Ok(result);
        }

        [HttpGet("byCategory/data")]
        public async Task<ActionResult<List<SearchFileDataDTO>>> SearchFilesByCategory([FromQuery][Required] string category, CancellationToken cancellationToken)
        {
            var result = await _searchFileService.GetFileDataByCategoryAsync(category, cancellationToken);
            return Ok(result);
        }

        [HttpGet("byName/data")]
        public async Task<ActionResult<List<SearchFileDataDTO>>> SearchFilesByName([FromQuery][Required] string name, CancellationToken cancellationToken)
        {
            var result = await _searchFileService.GetFileDataByNameAsync(name, cancellationToken);
            return Ok(result);
        }

        [HttpGet("byDescription/data")]
        public async Task<ActionResult<List<SearchFileDataDTO>>> SearchFilesByDescription([FromQuery][Required] string description, CancellationToken cancellationToken)
        {
            var result = await _searchFileService.GetFileDataByDescriptionAsync(description, cancellationToken);
            return Ok(result);
        }
    }
}
