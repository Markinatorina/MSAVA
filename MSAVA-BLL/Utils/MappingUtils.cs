using MSAVA_Shared.Models;
using MSAVA_DAL.Models;
using MSAVA_DAL.Utils;
using MSAVA_INF.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace MSAVA_BLL.Utils
{
    public static class MappingUtils
    {
        public static FileExtensionType ParseFileExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return FileExtensionType.Unknown;

            string normalized = extension.Trim().TrimStart('.').ToUpperInvariant();
            string enumName = $"_{normalized}";

            if (Enum.TryParse<FileExtensionType>(enumName, out FileExtensionType result))
                return result;

            return FileExtensionType.Unknown;
        }

        public static UserDTO MapUserDTOWithRelationships(UserDB db)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));
            if (db.AccessGroups == null)
                throw new ArgumentException("AccessGroups cannot be null", nameof(db));

            List<AccessGroupDTO> accessGroups = new List<AccessGroupDTO>();
            foreach (AccessGroupDB accessGroup in db.AccessGroups)
            {
                accessGroups.Add(MapAccessGroupDTO(accessGroup));
            }

            UserDTO userDTO = MapUserDTO(db);
            userDTO.AccessGroups = accessGroups;

            return userDTO;
        }

        public static UserDTO MapUserDTO(UserDB db)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));
            return new UserDTO
            {
                Id = db.Id,
                Username = db.Username,
                IsAdmin = db.IsAdmin,
                IsBanned = db.IsBanned,
                IsWhitelisted = db.IsWhitelisted,
                CreatedAt = db.CreatedAt
            };
        }

        public static AccessGroupDTO MapAccessGroupDTO(AccessGroupDB db)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));
            return new AccessGroupDTO
            {
                Id = db.Id,
                Name = db.Name,
                CreatedAt = db.CreatedAt,
                OwnerId = db.OwnerId
            };
        }

        public static SavedFileReferenceDB MapSavedFileReferenceDB(
            SaveFileFromStreamDTO dto,
            byte[] fileHash,
            ulong fileLength)
        {
            FileExtensionType extension = MappingUtils.ParseFileExtension(dto.FileExtension);
            return new SavedFileReferenceDB
            {
                Id = Guid.NewGuid(),
                FileHash = fileHash,
                FileExtension = extension,
                PublicDownload = dto.PublicDownload,
                AccessGroupId = dto.AccessGroupId
            };
        }

        public static SavedFileReferenceDB MapSavedFileReferenceDB(
            SaveFileFromFetchDTO dto,
            byte[] fileHash,
            ulong fileLength)
        {
            FileExtensionType extension = MappingUtils.ParseFileExtension(dto.FileExtension);
            return new SavedFileReferenceDB
            {
                Id = Guid.NewGuid(),
                FileHash = fileHash,
                FileExtension = extension,
                PublicDownload = dto.PublicDownload,
                AccessGroupId = dto.AccessGroupId
            };
        }

        public static StreamReturnFileDTO MapReturnFileDTO(SavedFileReferenceDB db, byte[]? fileBytes = null, Stream? fileStream = null)
        {
            string fileName = GetFileName(db);

            string fileExtension = FileExtensionUtils.GetFileExtension(db);

            string contentType = MetadataUtils.GetContentType(fileExtension);

            Stream stream;
            if (fileBytes != null && fileBytes.Length > 0)
            {
                stream = new MemoryStream(fileBytes);
            }
            else if (fileStream != null && fileStream.Length > 0)
            {
                stream = fileStream;
                if (stream.CanSeek)
                    stream.Position = 0;
            }
            else
            {
                throw new ArgumentException("Either fileBytes or fileStream must be provided.");
            }

            return new StreamReturnFileDTO
            {
                Id = db.Id,
                FileName = fileName,
                FileExtension = fileExtension,
                FileStream = stream,
            };
        }

        public static string GetFileName(SavedFileReferenceDB db)
        {
            return BitConverter.ToString(db.FileHash).Replace("-", "").ToLowerInvariant();
        }

        public static SavedFileDataDB MapSavedFileDataDB(
            SaveFileFromStreamDTO dto,
            SavedFileReferenceDB savedFileDb,
            ulong sizeInBytes,
            Guid originalCreator,
            Guid lastModifiedBy
        )
        {
            string checksum = BitConverter.ToString(savedFileDb.FileHash).Replace("-", "").ToLowerInvariant();
            string mimeType = MetadataUtils.GetContentType(dto.FileExtension);
            string[] tags = (dto.Tags ?? new List<string>()).ToArray();
            string[] categories = (dto.Categories ?? new List<string>()).ToArray();
            JsonDocument metadata = MetadataUtils.ExtractMetadataFromFileStream(dto.Stream, dto.FileExtension);

            return new SavedFileDataDB
            {
                Id = Guid.NewGuid(),
                FileReferenceId = savedFileDb.Id,
                SizeInBytes = sizeInBytes,
                SavedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow,
                LastModifiedById = lastModifiedBy,
                Checksum = checksum,
                Name = dto.FileName,
                Description = dto.Description ?? string.Empty,
                MimeType = mimeType,
                FileExtension = dto.FileExtension,
                Tags = tags,
                Categories = categories,
                OriginalCreator = originalCreator,
                Metadata = metadata,
                PublicViewing = dto.PublicViewing,
                DownloadCount = 0,
            };
        }

        public static SavedFileDataDB MapSavedFileDataDB(
            SaveFileFromFetchDTO dto,
            SavedFileReferenceDB savedFileDb,
            ulong sizeInBytes,
            Guid originalCreator,
            Guid lastModifiedBy
        )
        {
            string checksum = BitConverter.ToString(savedFileDb.FileHash).Replace("-", "").ToLowerInvariant();
            string mimeType = MetadataUtils.GetContentType(dto.FileExtension);
            string[] tags = (dto.Tags ?? new List<string>()).ToArray();
            string[] categories = (dto.Categories ?? new List<string>()).ToArray();
            // Todo: Extract metadata from the temp file
            JsonDocument metadata = JsonDocument.Parse("{}", new JsonDocumentOptions());

            return new SavedFileDataDB
            {
                Id = Guid.NewGuid(),
                FileReferenceId = savedFileDb.Id,
                SizeInBytes = sizeInBytes,
                SavedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow,
                LastModifiedById = lastModifiedBy,
                Checksum = checksum,
                Name = dto.FileName,
                Description = dto.Description ?? string.Empty,
                MimeType = mimeType,
                FileExtension = dto.FileExtension,
                Tags = tags,
                Categories = categories,
                OriginalCreator = originalCreator,
                Metadata = metadata,
                PublicViewing = dto.PublicViewing,
                DownloadCount = 0,
            };
        }

        public static SearchFileDataDTO MapSearchFileDataDTO(SavedFileDataDB db)
        {
            if (db.FileReference == null)
            {
                throw new ArgumentException("FileReference cannot be null", nameof(db));
            }
            string fileExtension = FileExtensionUtils.GetFileExtension(db.FileReference);
            string fileName = MappingUtils.GetFileName(db.FileReference);
            string filePath = Path.Combine(fileName, fileExtension);

            return new SearchFileDataDTO
            {
                DataId = db.Id,
                RefId = db.FileReference?.Id ?? Guid.Empty,
                FilePath = filePath,
                Name = db.Name,
                Description = db.Description,
                MimeType = db.MimeType,
                FileExtension = fileExtension,
                Tags = db.Tags,
                Categories = db.Categories,
                SizeInBytes = db.SizeInBytes,
                Checksum = db.Checksum,
                Metadata = db.Metadata,
                PublicViewing = db.PublicViewing,
                DownloadCount = db.DownloadCount,
                SavedAt = db.SavedAt,
                LastModifiedAt = db.LastModifiedAt,
                LastModifiedById = db.LastModifiedById
            };
        }

        public static SavedFileMetaJSON MapSavedFileMetaJSON(SavedFileReferenceDB db)
        {
            return new SavedFileMetaJSON
            {
                RefId = db.Id,
                PublicDownload = db.PublicDownload,
                AccessGroupId = db.AccessGroupId
            };
        }
    }
}
