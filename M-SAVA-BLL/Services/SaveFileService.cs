using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using System.IO;
using System.Threading;
using M_SAVA_Core.Models;
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

namespace M_SAVA_BLL.Services
{
    public class SaveFileService : ISaveFileService
    {
        private readonly IIdentifiableRepository<SavedFileReferenceDB> _savedRefsRepository;
        private readonly IIdentifiableRepository<SavedFileDataDB> _savedDataRepository;
        private readonly FileManager _fileManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ServiceLogger _serviceLogger;
        private readonly IHttpClientFactory _httpClientFactory;

        public SaveFileService(
            IIdentifiableRepository<SavedFileReferenceDB> savedFileRepository,
            IIdentifiableRepository<SavedFileDataDB> savedDataRepository,
            IUserService userService,
            FileManager fileManager,
            IHttpContextAccessor httpContextAccessor,
            ServiceLogger serviceLogger,
            IHttpClientFactory httpClientFactory)
        {
            _savedRefsRepository = savedFileRepository ?? throw new ArgumentNullException(nameof(savedFileRepository), "Service: savedFileRepository cannot be null.");
            _savedDataRepository = savedDataRepository ?? throw new ArgumentNullException(nameof(savedDataRepository), "Service: savedDataRepository cannot be null.");
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager), "Service: fileManager cannot be null.");
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor), "Service: httpContextAccessor cannot be null.");
            _serviceLogger = serviceLogger ?? throw new ArgumentNullException(nameof(serviceLogger), "Service: serviceLogger cannot be null.");
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory), "Service: httpClientFactory cannot be null.");
        }

        public async Task<Guid> CreateFileFromStreamAsync(SaveFileFromStreamDTO dto, CancellationToken cancellationToken = default)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.FileName))
                throw new ArgumentException("FileName must be provided in the DTO.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.FileExtension))
                throw new ArgumentException("FileExtension must be provided in the DTO.", nameof(dto));
            if (dto.Stream == null)
                throw new ArgumentException("Stream must be provided in the DTO.", nameof(dto));
            if (dto.AccessGroupId == Guid.Empty)
                throw new ArgumentException("AccessGroupId must be provided in the DTO.", nameof(dto));
            if (dto.Tags == null)
                dto.Tags = new List<string>();
            if (dto.Categories == null)
                dto.Categories = new List<string>();
            if (dto.Description == null)
                dto.Description = string.Empty;

            Guid sessionUserId = Guid.Empty;
            HttpContext httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Items["SessionDTO"] is SessionDTO sessionDto)
            {
                sessionUserId = sessionDto.UserId;
            }

            string tempFilePath = Path.GetTempFileName();
            long fileLength = 0;
            byte[] fileHash;
            using (var hashAlgorithm = SHA256.Create())
            {
                using (var tempFileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var cryptoStream = new CryptoStream(tempFileStream, hashAlgorithm, CryptoStreamMode.Write))
                {
                    byte[] buffer = new byte[81920];
                    int bytesRead;
                    while ((bytesRead = await dto.Stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        await cryptoStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                        fileLength += bytesRead;
                    }
                    await cryptoStream.FlushAsync(cancellationToken);
                }
                fileHash = hashAlgorithm.Hash ?? throw new InvalidOperationException("Hash computation failed: hashAlgorithm.Hash is null.");
            }

            SavedFileReferenceDB savedFileDb = MappingUtils.MapSavedFileReferenceDB(
                dto,
                fileHash,
                (ulong)fileLength
            );
            savedFileDb.Id = Guid.NewGuid();

            SavedFileMetaJSON savedFileMetaJSON = MappingUtils.MapSavedFileMetaJSON(savedFileDb);

            await _fileManager.SaveTempFileAsync(
                savedFileMetaJSON,
                savedFileDb.FileHash,
                savedFileDb.FileExtension.ToString(),
                tempFilePath,
                cancellationToken
            );

            SavedFileDataDB savedFileDataDb = MappingUtils.MapSavedFileDataDB(
                dto,
                savedFileDb,
                (ulong)fileLength,
                sessionUserId,
                sessionUserId
            );

            _savedRefsRepository.Insert(savedFileDb);
            await _savedRefsRepository.CommitAsync();

            _savedDataRepository.Insert(savedFileDataDb);
            await _savedDataRepository.CommitAsync();

            string fileName = MappingUtils.GetFileName(savedFileDb);
            string fileExtension = FileExtensionUtils.GetFileExtension(savedFileDb);
            string fileNameWithExtension = $"{fileName}{fileExtension}";

            _serviceLogger.WriteLog(AccessLogActions.NewFileCreated, $"File created: {fileNameWithExtension}", sessionUserId, fileNameWithExtension, savedFileDb.Id);

            return savedFileDb.Id;
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

            return await CreateFileFromStreamAsync(streamDto, cancellationToken);
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

            return await CreateFileFromStreamAsync(streamDto, cancellationToken);
        }
    }
}