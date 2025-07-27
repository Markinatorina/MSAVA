using M_SAVA_INF.Models;
using M_SAVA_INF.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace M_SAVA_INF.Managers
{
    public class FileManager
    {
        public FileManager()
        {
        }

        public async Task SaveFileContentAsync(SavedFileMetaJSON fileMeta, byte[] fileHash, string fileExtension, Stream contentStream, CancellationToken cancellationToken = default, bool overwrite = false)
        {
            if (contentStream == null) throw new ArgumentNullException(nameof(contentStream));
            if (!contentStream.CanSeek)
            {
                MemoryStream ms = new MemoryStream();
                await contentStream.CopyToAsync(ms, cancellationToken);
                ms.Position = 0;
                contentStream = ms;
            }
            else
            {
                contentStream.Position = 0;
            }

            bool isValid = FileContentUtils.ValidateFileContent(contentStream, fileExtension);
            if (!isValid)
                throw new ArgumentException("File content does not match the provided extension.");

            if (contentStream.CanSeek)
                contentStream.Position = 0;

            string path = FileContentUtils.GetFullPath(fileHash, fileExtension);
            bool fileExists = File.Exists(path);
            if (overwrite || !fileExists)
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                {
                    await contentStream.CopyToAsync(fileStream, cancellationToken);
                }
            }

            string metaPath = path + ".meta.json";
            List<SavedFileMetaJSON> metaList = new List<SavedFileMetaJSON>();
            if (File.Exists(metaPath))
            {
                var existingJson = await File.ReadAllTextAsync(metaPath, cancellationToken);
                if (!string.IsNullOrWhiteSpace(existingJson))
                {
                    try
                    {
                        var existingList = JsonSerializer.Deserialize<List<SavedFileMetaJSON>>(existingJson);
                        if (existingList != null)
                            metaList.AddRange(existingList);
                    }
                    catch { /* ignore corrupted data, continue */ }
                }
            }
            metaList.Add(fileMeta);
            var metaJson = JsonSerializer.Serialize(metaList);
            await File.WriteAllTextAsync(metaPath, metaJson, cancellationToken);
        }

        public FileStream GetFileStream(string fileNameWithExtension)
        {
            if(FileContentUtils.IsSafeFileName(fileNameWithExtension))
                throw new UnauthorizedAccessException($"Unsafe file name: {fileNameWithExtension}");
            string fullPath = FileContentUtils.GetFullPath(fileNameWithExtension);
            if(FileContentUtils.IsSafeFilePath(fullPath) == false)
                throw new UnauthorizedAccessException($"Unsafe file path: {fullPath}");
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"File not found: {fullPath}");

            FileStreamOptions options = FileStreamUtils.GetDefaultFileStreamOptions();
            return GetFileStream(fullPath, options);
        }
        public FileStream GetFileStream(byte[] fileHash, string fileExtension)
        {
            string fullPath = FileContentUtils.GetFullPath(fileHash, fileExtension);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"File not found: {fullPath}");

            FileStreamOptions options = FileStreamUtils.GetDefaultFileStreamOptions();
            return GetFileStream(fullPath, options);
        }
        public FileStream GetFileStream(string fullPath, FileStreamOptions options)
        {
            return new FileStream(fullPath, options);
        }

        public PhysicalFileResult GetPhysicalFile(string fileNameWithExtension, string contentType)
        {
            string fullPath = FileContentUtils.GetFullPath(fileNameWithExtension);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"File not found: {fullPath}");

            var fileName = Path.GetFileName(fullPath);
            return new PhysicalFileResult(fullPath, contentType)
            {
                FileDownloadName = fileName,
                EnableRangeProcessing = true
            };
        }

        public bool FileExists(byte[] fileHash, string fileExtension)
        {
            string path = FileContentUtils.GetFullPath(fileHash, fileExtension);
            return File.Exists(path);
        }

        public void DeleteFileContent(byte[] fileHash, string fileExtension)
        {
            string path = FileContentUtils.GetFullPath(fileHash, fileExtension);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public bool CheckFileAccessByPath(string fileNameWithExtension, List<Guid> userAccessGroups)
        {
            string fullPath = FileContentUtils.GetFullPath(fileNameWithExtension);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"The file '{fullPath}' does not exist.");
            }

            string metaPath = fullPath + ".meta.json";

            if (!File.Exists(metaPath))
            {
                throw new UnauthorizedAccessException($"Meta file '{metaPath}' does not exist.");
            }

            var metaJson = File.ReadAllText(metaPath);
            var metaList = JsonSerializer.Deserialize<List<SavedFileMetaJSON>>(metaJson);

            if (metaList == null || metaList.Count == 0)
            {
                throw new FileNotFoundException($"Meta file '{metaPath}' is invalid or empty.");
            }

            bool hasAccess = metaList.Any(meta => meta.PublicDownload || (userAccessGroups != null && userAccessGroups.Contains(meta.AccessGroupId)));
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("User does not have access to this file's access group.");
            }
            return true;
        }
    }
}
