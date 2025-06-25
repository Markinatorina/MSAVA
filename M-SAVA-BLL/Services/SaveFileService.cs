using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using System.IO;
using System.Threading;
using M_SAVA_BLL.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using M_SAVA_BLL.Utils;

namespace M_SAVA_BLL.Services
{
    public class SaveFileService
    {
        private readonly ISavedFileRepository _savedFileRepository;

        public SaveFileService(ISavedFileRepository savedFileRepository)
        {
            _savedFileRepository = savedFileRepository ?? throw new ArgumentNullException(nameof(savedFileRepository));
        }

        // Create
        public Guid CreateFile(FileToSaveDTO dto)
        {
            SavedFileDB savedFileDb = FileUtils.MapFileDTOToDB(dto);

            if (_savedFileRepository.FileExistsByHashAndExtension(savedFileDb.FileHash, savedFileDb.FileExtension))
            {
                return _savedFileRepository.GetFileIdByHashAndExtension(savedFileDb.FileHash, savedFileDb.FileExtension);
            }

            savedFileDb.Id = Guid.NewGuid();

            _savedFileRepository.Insert(savedFileDb);
            _savedFileRepository.Commit();

            SaveFileContent(savedFileDb, dto, false);

            return savedFileDb.Id;
        }

        // Read
        public FileToSaveDTO GetFileById(Guid id, bool asBytes = false)
        {
            SavedFileDB db = _savedFileRepository.GetById(id);

            FileToSaveDTO dto = new FileToSaveDTO
            {
                Id = db.Id,
                FileHash = db.FileHash,
                FileExtension = db.FileExtension.ToString().TrimStart('_')
            };

            if (asBytes)
            {
                byte[] fileBytes = _savedFileRepository.GetFileBytes(db);
                dto.Bytes = fileBytes;
            }
            else
            {
                FileStream fileStream = _savedFileRepository.GetFileStream(db);
                dto.Stream = fileStream;
            }

            return dto;
        }

        // Update
        public void UpdateFile(FileToSaveDTO dto)
        {
            SavedFileDB savedFileDb = FileUtils.MapFileDTOToDB(dto);

            _savedFileRepository.Update(savedFileDb);
            _savedFileRepository.Commit();

            SaveFileContent(savedFileDb, dto, true);
        }

        // Delete
        public void DeleteFile(Guid id)
        {
            _savedFileRepository.DeleteById(id);
            _savedFileRepository.Commit();
        }

        // Helper to save file content (sync)
        private void SaveFileContent(SavedFileDB savedFileDb, FileToSaveDTO dto, bool overwrite)
        {
            if (dto.Stream != null && dto.Stream.Length > 0)
            {
                dto.Stream.Position = 0;
                _savedFileRepository.SaveFileFromStream(savedFileDb, dto.Stream, overwrite);
            }
            else if (dto.FormFile != null && dto.FormFile.Length > 0)
            {
                using (Stream stream = dto.FormFile.OpenReadStream())
                {
                    _savedFileRepository.SaveFileFromStream(savedFileDb, stream, overwrite);
                }
            }
            else if (dto.Bytes != null && dto.Bytes.Length > 0)
            {
                _savedFileRepository.SaveFileFromBytes(savedFileDb, dto.Bytes, overwrite);
            }
            else
            {
                throw new ArgumentException("No file content provided.");
            }
        }

        // Async CRUD methods
        // Create
        public async Task<Guid> CreateFileAsync(FileToSaveDTO dto, CancellationToken cancellationToken = default)
        {
            SavedFileDB savedFileDb = FileUtils.MapFileDTOToDB(dto);

            if (_savedFileRepository.FileExistsByHashAndExtension(savedFileDb.FileHash, savedFileDb.FileExtension))
            {
                return _savedFileRepository.GetFileIdByHashAndExtension(savedFileDb.FileHash, savedFileDb.FileExtension);
            }

            savedFileDb.Id = Guid.NewGuid();

            _savedFileRepository.Insert(savedFileDb);
            await _savedFileRepository.CommitAsync();

            await SaveFileContentAsync(savedFileDb, dto, false, cancellationToken);

            return savedFileDb.Id;
        }

        // Read
        public async Task<FileToSaveDTO> GetFileByIdAsync(Guid id, bool asBytes = false, CancellationToken cancellationToken = default)
        {
            SavedFileDB db = await _savedFileRepository.GetByIdAsync(id, cancellationToken);

            FileToSaveDTO dto = new FileToSaveDTO
            {
                Id = db.Id,
                FileHash = db.FileHash,
                FileExtension = db.FileExtension.ToString().TrimStart('_')
            };

            if (asBytes)
            {
                byte[] fileBytes = _savedFileRepository.GetFileBytes(db);
                dto.Bytes = fileBytes;
            }
            else
            {
                FileStream fileStream = _savedFileRepository.GetFileStream(db);
                dto.Stream = fileStream;
            }

            return dto;
        }

        // Update
        public async Task UpdateFileAsync(FileToSaveDTO dto, CancellationToken cancellationToken = default)
        {
            SavedFileDB savedFileDb = FileUtils.MapFileDTOToDB(dto);

            _savedFileRepository.Update(savedFileDb);
            await _savedFileRepository.CommitAsync();

            await SaveFileContentAsync(savedFileDb, dto, true, cancellationToken);
        }

        // Delete
        public async Task DeleteFileAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _savedFileRepository.DeleteByIdAsync(id);
            await _savedFileRepository.CommitAsync();
        }

        // Helper to save file content (async)
        private async Task SaveFileContentAsync(SavedFileDB savedFileDb, FileToSaveDTO dto, bool overwrite, CancellationToken cancellationToken = default)
        {
            if (dto.Stream != null && dto.Stream.Length > 0)
            {
                dto.Stream.Position = 0;
                await _savedFileRepository.SaveFileFromStreamAsync(savedFileDb, dto.Stream, overwrite, cancellationToken);
            }
            else if (dto.FormFile != null && dto.FormFile.Length > 0)
            {
                await _savedFileRepository.SaveFileFromFormFileAsync(savedFileDb, dto.FormFile, overwrite, cancellationToken);
            }
            else if (dto.Bytes != null && dto.Bytes.Length > 0)
            {
                await _savedFileRepository.SaveFileFromStreamAsync(savedFileDb, new MemoryStream(dto.Bytes), overwrite, cancellationToken);
            }
            else
            {
                throw new ArgumentException("No file content provided.");
            }
        }
    }
}