using MSAVA_BLL.Loggers;
using MSAVA_BLL.Services.Persistence;
using MSAVA_Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MSAVA_BLL.Services.Fetch
{
    public class FetchOneDriveService
    {
        private readonly FileWriteService _saveFileService;
        private readonly ServiceLogger _serviceLogger;

        public FetchOneDriveService(FileWriteService saveFileService, ServiceLogger serviceLogger)
        {
            _saveFileService = saveFileService;
            _serviceLogger = serviceLogger ?? throw new ArgumentNullException(nameof(serviceLogger));
        }

        public async Task<Guid> NoAuthFileFetch(FetchFileFromOneDriveDTO dto, CancellationToken cancellationToken = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.FileUrl))
                throw new ArgumentException("FileUrl must be provided.", nameof(dto));
            if (dto.AccessGroupId == Guid.Empty) throw new ArgumentException("AccessGroupId must be provided.", nameof(dto));

            string downloadUrl;
            using var http = new HttpClient();

            var shareId = "u!" + Base64UrlEncode(dto.FileUrl);
            downloadUrl = $"https://api.onedrive.com/v1.0/shares/{shareId}/root/content";

            _serviceLogger.LogInformation($"Starting OneDrive download. URL: {downloadUrl}");

            using var resp = await http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                _serviceLogger.WriteLog((int)resp.StatusCode, $"OneDrive download returned {(int)resp.StatusCode}: {body}", null);
                throw new InvalidOperationException($"OneDrive download failed {(int)resp.StatusCode}: {body}");
            }

            var respMediaType = resp.Content.Headers.ContentType?.MediaType ?? string.Empty;
            if (respMediaType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
            {
                var html = await resp.Content.ReadAsStringAsync(cancellationToken);
                _serviceLogger.WriteLog(400, $"OneDrive returned HTML instead of binary content. Snippet: {html?.Substring(0, Math.Min(200, html.Length))}", null);
                throw new InvalidOperationException("OneDrive returned HTML instead of file content. The file may require authentication or be a preview page.");
            }

            var tempFilePath = Path.GetTempFileName();
            _serviceLogger.LogInformation($"Downloading OneDrive content to temp path {tempFilePath}");

            using (var contentStream = await resp.Content.ReadAsStreamAsync(cancellationToken))
            using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await contentStream.CopyToAsync(fs, cancellationToken);
            }

            string finalFileName = dto.Description ?? $"OneDrive File";
            string inferredExtension = GetExtensionFromResponse(resp);

            if (resp.Content.Headers.ContentDisposition != null)
            {
                var cd = resp.Content.Headers.ContentDisposition;
                var fileName = cd.FileNameStar ?? cd.FileName;
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = fileName.Trim('"');
                    finalFileName = Path.GetFileNameWithoutExtension(fileName);
                    var fileExt = Path.GetExtension(fileName).TrimStart('.');
                    if (!string.IsNullOrWhiteSpace(fileExt))
                        inferredExtension = fileExt;
                }
            }

            if (string.IsNullOrWhiteSpace(inferredExtension))
                inferredExtension = "bin";

            var fetchDto = new SaveFileFromFetchDTO
            {
                FileName = finalFileName,
                FileExtension = inferredExtension,
                TempFilePath = tempFilePath,
                AccessGroupId = dto.AccessGroupId,
                Tags = dto.Tags ?? new List<string>(),
                Categories = dto.Categories ?? new List<string>(),
                Description = dto.Description ?? string.Empty,
                PublicViewing = dto.PublicViewing,
                PublicDownload = dto.PublicDownload
            };

            return await _saveFileService.CreateFileFromTempFileAsync(fetchDto, cancellationToken);
        }

        private static string Base64UrlEncode(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var base64 = Convert.ToBase64String(bytes);
            return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static string GetExtensionFromResponse(HttpResponseMessage resp)
        {
            var mediaType = resp.Content.Headers.ContentType?.MediaType ?? string.Empty;
            var ext = GetExtensionFromContentType(mediaType);
            if (!string.IsNullOrWhiteSpace(ext))
                return ext;

            if (resp.Content.Headers.ContentDisposition != null)
            {
                var cd = resp.Content.Headers.ContentDisposition;
                var fileName = cd.FileNameStar ?? cd.FileName;
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    var fileExt = Path.GetExtension(fileName).TrimStart('.');
                    if (!string.IsNullOrWhiteSpace(fileExt))
                        return fileExt;
                }
            }

            return string.Empty;
        }

        private static string GetExtensionFromContentType(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType)) return string.Empty;
            contentType = contentType.ToLowerInvariant();

            return contentType switch
            {
                "video/mp4" => "mp4",
                "video/webm" => "webm",
                "audio/mpeg" => "mp3",
                "audio/mp3" => "mp3",
                "audio/ogg" => "ogg",
                "image/png" => "png",
                "image/jpeg" => "jpg",
                "application/pdf" => "pdf",
                "application/zip" => "zip",
                "application/octet-stream" => "bin",
                "text/plain" => "txt",
                _ when contentType.Contains("mp4") => "mp4",
                _ when contentType.Contains("mpeg") => "mp3",
                _ when contentType.Contains("jpeg") || contentType.Contains("jpg") => "jpg",
                _ when contentType.Contains("png") => "png",
                _ when contentType.Contains("pdf") => "pdf",
                _ => string.Empty
            };
        }
    }
}
