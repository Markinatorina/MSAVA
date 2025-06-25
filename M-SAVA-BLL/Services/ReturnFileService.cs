using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using M_SAVA_BLL.Models;
using M_SAVA_BLL.Utils;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace M_SAVA_BLL.Services
{
    public class ReturnFileService
    {
        private readonly ISavedFileRepository _savedFileRepository;

        public ReturnFileService(ISavedFileRepository savedFileRepository)
        {
            _savedFileRepository = savedFileRepository ?? throw new ArgumentNullException(nameof(savedFileRepository));
        }

        public ReturnFileDTO GetFileById(Guid id)
        {
            SavedFileDB db = _savedFileRepository.GetById(id);
            FileStream fileStream = _savedFileRepository.GetFileStream(db);
            return FileUtils.MapDBToReturnFileDTO(db, fileStream: fileStream);
        }

        public async Task<ReturnFileDTO> GetFileByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            SavedFileDB db = await _savedFileRepository.GetByIdAsync(id, cancellationToken);
            FileStream fileStream = await _savedFileRepository.GetFileStreamAsync(db, cancellationToken: cancellationToken);
            return FileUtils.MapDBToReturnFileDTO(db, fileStream: fileStream);
        }

        public FileStream GetFileStreamById(Guid id)
        {
            SavedFileDB db = _savedFileRepository.GetById(id);
            return _savedFileRepository.GetFileStream(db);
        }

        public async Task<FileStream> GetFileStreamByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            SavedFileDB db = await _savedFileRepository.GetByIdAsync(id, cancellationToken);
            return await _savedFileRepository.GetFileStreamAsync(db, cancellationToken: cancellationToken);
        }
    }
}
