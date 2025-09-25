using M_SAVA_Shared.Models;
using M_SAVA_DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using M_SAVA_BLL.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

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
        public ActionResult<List<Guid>> SearchFileGuidsByAllFields([FromQuery] string? tag, [FromQuery] string? category, [FromQuery] string? name, [FromQuery] string? description)
        {
            return Ok(_searchFileService.GetFileGuidsByAllFields(tag, category, name, description));
        }

        [HttpGet("byTag/id")]
        public ActionResult<List<Guid>> SearchFileGuidsByTag([FromQuery][Required] string tag)
        {
            return Ok(_searchFileService.GetFileGuidsByTag(tag));
        }

        [HttpGet("byCategory/id")]
        public ActionResult<List<Guid>> SearchFileGuidsByCategory([FromQuery][Required] string category)
        {
            return Ok(_searchFileService.GetFileGuidsByCategory(category));
        }

        [HttpGet("byName/id")]
        public ActionResult<List<Guid>> SearchFileGuidsByName([FromQuery][Required] string name)
        {
            return Ok(_searchFileService.GetFileGuidsByName(name));
        }

        [HttpGet("byDescription/id")]
        public ActionResult<List<Guid>> SearchFileGuidsByDescription([FromQuery][Required] string description)
        {
            return Ok(_searchFileService.GetFileGuidsByDescription(description));
        }

        [HttpGet("byAll/data")]
        public ActionResult<List<SearchFileDataDTO>> SearchFilesByAllFields([FromQuery] string? tag, [FromQuery] string? category, [FromQuery] string? name, [FromQuery] string? description)
        {
            return Ok(_searchFileService.GetFileDataByAllFields(tag, category, name, description));
        }

        [HttpGet("byTag/data")]
        public ActionResult<List<SearchFileDataDTO>> SearchFilesByTag([FromQuery][Required] string tag)
        {
            return Ok(_searchFileService.GetFileDataByTag(tag));
        }

        [HttpGet("byCategory/data")]
        public ActionResult<List<SearchFileDataDTO>> SearchFilesByCategory([FromQuery][Required] string category)
        {
            return Ok(_searchFileService.GetFileDataByCategory(category));
        }

        [HttpGet("byName/data")]
        public ActionResult<List<SearchFileDataDTO>> SearchFilesByName([FromQuery][Required] string name)
        {
            return Ok(_searchFileService.GetFileDataByName(name));
        }

        [HttpGet("byDescription/data")]
        public ActionResult<List<SearchFileDataDTO>> SearchFilesByDescription([FromQuery][Required] string description)
        {
            return Ok(_searchFileService.GetFileDataByDescription(description));
        }
    }
}
