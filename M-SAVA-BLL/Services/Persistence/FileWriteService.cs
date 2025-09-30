using M_SAVA_BLL.Loggers;
using M_SAVA_BLL.Utils;
using M_SAVA_Shared.Models;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Utils;
using M_SAVA_INF.Managers;
using M_SAVA_INF.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using M_SAVA_DAL.Repositories.Generic;

namespace M_SAVA_BLL.Services.Persistence
{
    public class FileWriteService
    {
        private readonly IIdentifiableRepository<SavedFileReferenceDB> _savedRefsRepository;
        private readonly IIdentifiableRepository<SavedFileDataDB> _savedDataRepository;
        private readonly FileManager _fileManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ServiceLogger _serviceLogger;

        public FileWriteService(
            IIdentifiableRepository<SavedFileReferenceDB> savedFileRepository,
            IIdentifiableRepository<SavedFileDataDB> savedDataRepository,
            FileManager fileManager,
            IHttpContextAccessor httpContextAccessor,
            ServiceLogger serviceLogger)
        {
            _savedRefsRepository = savedFileRepository ?? throw new ArgumentNullException(nameof(savedFileRepository), "Service: savedFileRepository cannot be null.");
            _savedDataRepository = savedDataRepository ?? throw new ArgumentNullException(nameof(savedDataRepository), "Service: savedDataRepository cannot be null.");
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager), "Service: fileManager cannot be null.");
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor), "Service: httpContextAccessor cannot be null.");
            _serviceLogger = serviceLogger ?? throw new ArgumentNullException(nameof(serviceLogger), "Service: serviceLogger cannot be null.");
        }

        public async Task<Guid> CreateFileFromStreamAsync(SaveFileFromStreamDTO dto, CancellationToken cancellationToken = default)
        {
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

            // Insert both entities and persist once (same DbContext handles all tracked changes)
            _savedRefsRepository.Insert(savedFileDb);
            _savedDataRepository.Insert(savedFileDataDb);
            await _savedRefsRepository.SaveChangesAndDetachAsync();

            string fileName = MappingUtils.GetFileName(savedFileDb);
            string fileExtension = FileExtensionUtils.GetFileExtension(savedFileDb);
            string fileNameWithExtension = $"{fileName}{fileExtension}";

            _serviceLogger.WriteLog(AccessLogActions.NewFileCreated, $"File created: {fileNameWithExtension}", sessionUserId, fileNameWithExtension, savedFileDb.Id);

            return savedFileDb.Id;
        }

        public async Task<Guid> CreateFileFromTempFileAsync(
            SaveFileFromFetchDTO dto,
            CancellationToken cancellationToken = default)
        {
            Guid sessionUserId = Guid.Empty;
            HttpContext httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Items["SessionDTO"] is SessionDTO sessionDto)
            {
                sessionUserId = sessionDto.UserId;
            }

            string tempFilePath = dto.TempFilePath;
            try
            {
                long fileLength = new FileInfo(tempFilePath).Length;
                byte[] fileHash;
                using (var hashAlgorithm = SHA256.Create())
                {
                    using (var tempFileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var cryptoStream = new CryptoStream(tempFileStream, hashAlgorithm, CryptoStreamMode.Read))
                    {
                        byte[] buffer = new byte[81920];
                        int bytesRead;
                        while ((bytesRead = await cryptoStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                        {
                            // Just read to compute hash
                        }
                    }
                    fileHash = hashAlgorithm.Hash ?? throw new InvalidOperationException("Hash computation failed: hashAlgorithm.Hash is null.");
                }

                SavedFileReferenceDB savedFileDb = MappingUtils.MapSavedFileReferenceDB(
                    dto,
                    fileHash,
                    (ulong)fileLength
                );

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

                // Insert both entities and persist once
                _savedRefsRepository.Insert(savedFileDb);
                _savedDataRepository.Insert(savedFileDataDb);
                await _savedRefsRepository.SaveChangesAndDetachAsync();

                string fileName = MappingUtils.GetFileName(savedFileDb);
                string fileExtension = FileExtensionUtils.GetFileExtension(savedFileDb);
                string fileNameWithExtension = $"{fileName}{fileExtension}";

                _serviceLogger.WriteLog(AccessLogActions.NewFileCreated, $"File created: {fileNameWithExtension}", sessionUserId, fileNameWithExtension, savedFileDb.Id);

                return savedFileDb.Id;
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    try { File.Delete(tempFilePath); } catch { /* ignore */ }
                }
            }
        }
    }
}
