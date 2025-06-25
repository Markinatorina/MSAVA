using M_SAVA_DAL.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Repositories
{
    public interface ISavedFileRepository : IIdentifiableRepository<SavedFileDB>
    {
        string GetFilePath(SavedFileDB file);
        byte[] GetFileBytes(SavedFileDB file);
        FileStream GetFileStream(SavedFileDB file, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read);
        Task<FileStream> GetFileStreamAsync(SavedFileDB file, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, CancellationToken cancellationToken = default);
        bool FileExists(SavedFileDB file);

        public Guid GetFileIdByHashAndExtension(byte[] fileHash, FileExtensionType extension);
        public bool FileExistsByHashAndExtension(byte[] fileHash, FileExtensionType extension);

        void SaveFileFromBytes(SavedFileDB file, byte[] content, bool overwrite = false);
        void SaveFileFromStream(SavedFileDB file, Stream content, bool overwrite = false);
        Task SaveFileFromStreamAsync(SavedFileDB file, Stream content, bool overwrite = false, CancellationToken cancellationToken = default);
        Task SaveFileFromFormFileAsync(SavedFileDB file, IFormFile formFile, bool overwrite = false, CancellationToken cancellationToken = default);
    }
}
