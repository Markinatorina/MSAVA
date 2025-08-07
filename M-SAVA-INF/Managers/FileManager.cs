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
                string existingJson = await File.ReadAllTextAsync(metaPath, cancellationToken);
                if (!string.IsNullOrWhiteSpace(existingJson))
                {
                    try
                    {
                        List<SavedFileMetaJSON> existingList = JsonSerializer.Deserialize<List<SavedFileMetaJSON>>(existingJson) ?? new List<SavedFileMetaJSON>();
                        if (existingList != null)
                            metaList.AddRange(existingList);
                    }
                    catch { /* ignore corrupted data, continue */ }
                }
            }
            metaList.Add(fileMeta);
            string metaJson = JsonSerializer.Serialize(metaList);
            await File.WriteAllTextAsync(metaPath, metaJson, cancellationToken);
        }

        public async Task SaveTempFileAsync(
            SavedFileMetaJSON fileMeta,
            byte[] fileHash,
            string fileExtension,
            string tempFilePath,
            CancellationToken cancellationToken = default,
            bool overwrite = false)
        {
            if (string.IsNullOrWhiteSpace(tempFilePath))
                throw new ArgumentNullException(nameof(tempFilePath));
            if (!File.Exists(tempFilePath))
                throw new FileNotFoundException("Temporary file not found.", tempFilePath);

            string path = FileContentUtils.GetFullPath(fileHash, fileExtension);
            bool fileExists = File.Exists(path);

            try
            {
                using (var tempFileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bool isValid = FileContentUtils.ValidateFileContent(tempFileStream, fileExtension);
                    if (!isValid)
                        throw new ArgumentException("File content does not match the provided extension.");
                }

                if (overwrite || !fileExists)
                {
                    if (fileExists)
                        File.Delete(path);

                    File.Move(tempFilePath, path);
                }

                string metaPath = path + ".meta.json";
                List<SavedFileMetaJSON> metaList = new List<SavedFileMetaJSON>();
                if (File.Exists(metaPath))
                {
                    string existingJson = await File.ReadAllTextAsync(metaPath, cancellationToken);
                    if (!string.IsNullOrWhiteSpace(existingJson))
                    {
                        try
                        {
                            List<SavedFileMetaJSON> existingList = JsonSerializer.Deserialize<List<SavedFileMetaJSON>>(existingJson) ?? new List<SavedFileMetaJSON>();
                            if (existingList != null)
                                metaList.AddRange(existingList);
                        }
                        catch { /* ignore corrupted data, continue */ }
                    }
                }
                metaList.Add(fileMeta);
                string metaJson = JsonSerializer.Serialize(metaList);
                await File.WriteAllTextAsync(metaPath, metaJson, cancellationToken);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    try { File.Delete(tempFilePath); } catch { /* ignore */ }
                }
            }
        }

        public FileStream GetFileStream(string fileNameWithExtension)
        {
            string fullPath = FileContentUtils.GetFullPathIfSafe(fileNameWithExtension);

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

            string fileName = Path.GetFileName(fullPath);
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

        public Guid CheckFileAccessByPath(string fileNameWithExtension, List<Guid> userAccessGroups)
        {
            string fullPath = FileContentUtils.GetFullPathIfSafe(fileNameWithExtension);

            string metaPath = fullPath + ".meta.json";

            if (!File.Exists(metaPath))
            {
                throw new UnauthorizedAccessException($"Meta file '{metaPath}' does not exist.");
            }

            string metaJson = File.ReadAllText(metaPath);
            List<SavedFileMetaJSON> metaList = JsonSerializer.Deserialize<List<SavedFileMetaJSON>>(metaJson) ?? new List<SavedFileMetaJSON>();

            if (metaList == null || metaList.Count == 0)
            {
                throw new FileNotFoundException($"Meta file '{metaPath}' is invalid or empty.");
            }

            foreach (var meta in metaList)
            {
                if (meta.PublicDownload || (userAccessGroups != null && userAccessGroups.Contains(meta.AccessGroupId)))
                {
                    return meta.RefId;
                }
            }
            throw new UnauthorizedAccessException("User does not have permission to access this file.");
        }
    }
}
