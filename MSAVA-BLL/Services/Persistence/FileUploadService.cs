using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M_SAVA_DAL.Models;
using System.IO;
using System.Threading;
using M_SAVA_Shared.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using M_SAVA_BLL.Utils;
using System.Text.Json;
using M_SAVA_INF.Managers;
using M_SAVA_INF.Utils;
using M_SAVA_INF.Models;
using M_SAVA_BLL.Services.Interfaces;
using M_SAVA_BLL.Loggers;
using M_SAVA_DAL.Utils;
using System.Net.Http;

namespace M_SAVA_BLL.Services.Persistence
{
    public class FileUploadService : IStoreFileService
    {
        private readonly FileWriteService _saveFileService;
        private readonly IHttpClientFactory _httpClientFactory;

        public FileUploadService(
            FileWriteService saveFileService,
            IHttpClientFactory httpClientFactory)
        {
            _saveFileService = saveFileService ?? throw new ArgumentNullException(nameof(saveFileService), "Service: saveFileService cannot be null.");
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory), "Service: httpClientFactory cannot be null.");
        }

        public async Task<Guid> CreateFileFromStreamAsync(SaveFileFromStreamDTO dto, CancellationToken cancellationToken = default)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.FileName))
                throw new ArgumentException("FileName must be provided.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.FileExtension))
                throw new ArgumentException("FileExtension must be provided.", nameof(dto));
            if (dto.Stream == null)
                throw new ArgumentException("File content must be provided as a stream.", nameof(dto));
            if (dto.AccessGroupId == Guid.Empty)
                throw new ArgumentException("AccessGroupId must be provided.", nameof(dto));
            if (dto.Tags == null)
                dto.Tags = new List<string>();
            if (dto.Categories == null)
                dto.Categories = new List<string>();
            if (dto.Description == null)
                dto.Description = string.Empty;

            return await _saveFileService.CreateFileFromStreamAsync(dto, cancellationToken);
        }

        public async Task<Guid> CreateFileFromURLAsync(SaveFileFromUrlDTO dto, CancellationToken cancellationToken = default)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.FileUrl))
                throw new ArgumentException("FileUrl must be provided.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.FileName))
                throw new ArgumentException("FileName must be provided.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.FileExtension))
                throw new ArgumentException("FileExtension must be provided.", nameof(dto));
            if (dto.AccessGroupId == Guid.Empty)
                throw new ArgumentException("AccessGroupId must be provided.", nameof(dto));

            var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.GetAsync(dto.FileUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            var streamDto = new SaveFileFromStreamDTO
            {
                FileName = dto.FileName,
                FileExtension = dto.FileExtension,
                Stream = stream,
                AccessGroupId = dto.AccessGroupId,
                Tags = dto.Tags ?? new List<string>(),
                Categories = dto.Categories ?? new List<string>(),
                Description = dto.Description ?? string.Empty,
                PublicViewing = dto.PublicViewing,
                PublicDownload = dto.PublicDownload
            };

            return await _saveFileService.CreateFileFromStreamAsync(streamDto, cancellationToken);
        }

        public async Task<Guid> CreateFileFromFormFileAsync(SaveFileFromFormFileDTO dto, CancellationToken cancellationToken = default)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.FileName))
                throw new ArgumentException("FileName must be provided.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.FileExtension))
                throw new ArgumentException("FileExtension must be provided.", nameof(dto));
            if (dto.FormFile == null)
                throw new ArgumentException("FormFile must be provided.", nameof(dto));
            if (dto.AccessGroupId == Guid.Empty)
                throw new ArgumentException("AccessGroupId must be provided.", nameof(dto));

            await using var stream = dto.FormFile.OpenReadStream();
            var streamDto = new SaveFileFromStreamDTO
            {
                FileName = dto.FileName,
                FileExtension = dto.FileExtension,
                Stream = stream,
                AccessGroupId = dto.AccessGroupId,
                Tags = dto.Tags ?? new List<string>(),
                Categories = dto.Categories ?? new List<string>(),
                Description = dto.Description ?? string.Empty,
                PublicViewing = dto.PublicViewing,
                PublicDownload = dto.PublicDownload
            };

            return await _saveFileService.CreateFileFromStreamAsync(streamDto, cancellationToken);
        }
    }
}