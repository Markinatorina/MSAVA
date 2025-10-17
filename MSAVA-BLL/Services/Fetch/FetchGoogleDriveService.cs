using MSAVA_BLL.Loggers;
using MSAVA_BLL.Services.Persistence;
using MSAVA_Shared.Models;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MSAVA_BLL.Services.Fetch
{
    public class FetchGoogleDriveService
    {
        private readonly FileWriteService _saveFileService;
        private readonly ServiceLogger _serviceLogger;

        public FetchGoogleDriveService(FileWriteService saveFileService, ServiceLogger serviceLogger)
        {
            _saveFileService = saveFileService;
            _serviceLogger = serviceLogger ?? throw new ArgumentNullException(nameof(serviceLogger));
        }

        public async Task<Guid> NoAuthFileFetch(FetchFileGoogleDriveDTO dto, CancellationToken cancellationToken = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.FileUrl)) throw new ArgumentException("FileUrl must be provided.", nameof(dto));
            if (dto.AccessGroupId == Guid.Empty) throw new ArgumentException("AccessGroupId must be provided.", nameof(dto));

            string? fileId = ExtractDriveFileId(dto.FileUrl);
            if (string.IsNullOrWhiteSpace(fileId))
                throw new ArgumentException("Could not extract Google Drive file id from the provided FileUrl.", nameof(dto.FileUrl));

            string baseDownloadUrl = $"https://drive.google.com/uc?export=download&id={WebUtility.UrlEncode(fileId)}";

            using var http = new HttpClient();

            using var initialResp = await http.GetAsync(baseDownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!initialResp.IsSuccessStatusCode)
            {
                var body = await initialResp.Content.ReadAsStringAsync(cancellationToken);
                var headers = string.Join("; ", initialResp.Headers.Select(h => $"{h.Key}: {string.Join(",", h.Value)}"));
                var reqHeaders = string.Join("; ", http.DefaultRequestHeaders.Select(h => $"{h.Key}: {string.Join(",", h.Value)}"));
                _serviceLogger.WriteLog((int)initialResp.StatusCode, $"Google Drive initial request failed. URL: {baseDownloadUrl} | RequestHeaders: {reqHeaders} | ResponseHeaders: {headers} | Body: {body}", null);
                throw new InvalidOperationException($"Google Drive initial request failed {(int)initialResp.StatusCode}: {body}");
            }

            var contentType = initialResp.Content.Headers.ContentType?.MediaType ?? string.Empty;

            string? downloadUrl = null;
            if (contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
            {
                var html = await initialResp.Content.ReadAsStringAsync(cancellationToken);
                if (html.Contains("docs.google.com"))
                    throw new InvalidOperationException("The file is a native Google Docs/Sheets/Slides type and cannot be downloaded with this method.");

                string? confirmToken = null;
                if (initialResp.Headers.TryGetValues("Set-Cookie", out var cookies))
                {
                    foreach (var cookie in cookies)
                    {
                        var m = Regex.Match(cookie, @"download_warning_[^=]+=([^;]+)");
                        if (m.Success)
                        {
                            confirmToken = WebUtility.UrlEncode(m.Groups[1].Value);
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(confirmToken))
                {
                    var m2 = Regex.Match(html, @"confirm=([0-9A-Za-z_\-]+)");
                    if (m2.Success) confirmToken = WebUtility.UrlEncode(m2.Groups[1].Value);
                }

                if (!string.IsNullOrEmpty(confirmToken))
                {
                    downloadUrl = $"{baseDownloadUrl}&confirm={confirmToken}";
                }
                else
                {
                    downloadUrl = $"{baseDownloadUrl}&confirm=t";
                }
            }
            else
            {
                downloadUrl = baseDownloadUrl;
            }

            if (string.IsNullOrWhiteSpace(downloadUrl))
                throw new InvalidOperationException("Failed to build a download URL for the requested Google Drive file.");

            var tempFilePath = Path.GetTempFileName();
            _serviceLogger.LogInformation($"Downloading Google Drive file {_serviceLogger.SanitizeString(fileId)} to temp path {tempFilePath}");

            var contentTypeHeader = string.Empty;
            var finalExtension = "bin";
            var fetchDto = default(SaveFileFromFetchDTO);
            using (var downloadResp = await http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                downloadResp.EnsureSuccessStatusCode();

                var respMediaType = downloadResp.Content.Headers.ContentType?.MediaType ?? string.Empty;
                if (respMediaType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
                {
                    var body = await downloadResp.Content.ReadAsStringAsync(cancellationToken);
                    _serviceLogger.WriteLog(400, $"Google Drive returned HTML instead of file content. Maybe the file requires authentication or is a Google-native doc. Snippet: {body?.Substring(0, Math.Min(200, body.Length))}", null);
                    throw new InvalidOperationException("Google Drive returned an HTML page instead of file content. The file may require authentication or be a Google-native document (Docs/Sheets/Slides).");
                }

                using (var ms = await downloadResp.Content.ReadAsStreamAsync(cancellationToken))
                using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await ms.CopyToAsync(fs, cancellationToken);
                }
                contentTypeHeader = downloadResp.Content.Headers.ContentType?.MediaType ?? string.Empty;
                var inferredExtension = GetExtensionFromContentType(contentTypeHeader);
                finalExtension = string.IsNullOrWhiteSpace(inferredExtension) ? "bin" : inferredExtension;
                var fileName = dto.FileUrl;

                fetchDto = new SaveFileFromFetchDTO
                {
                    FileName = "Google Drive File",
                    FileExtension = finalExtension,
                    TempFilePath = tempFilePath,
                    AccessGroupId = dto.AccessGroupId,
                    Tags = dto.Tags ?? new List<string>(),
                    Categories = dto.Categories ?? new List<string>(),
                    Description = dto.Description ?? string.Empty,
                    PublicViewing = dto.PublicViewing,
                    PublicDownload = dto.PublicDownload
                };
            }
            return await _saveFileService.CreateFileFromTempFileAsync(fetchDto, cancellationToken);
        }

        private static string? ExtractDriveFileId(string urlOrId)
        {
            if (string.IsNullOrWhiteSpace(urlOrId))
                return null;

            // If it already looks like an ID (alphanumeric with - or _ and length between 10..100), return it
            if (Regex.IsMatch(urlOrId, @"^[A-Za-z0-9_\-]{10,100}$"))
                return urlOrId;

            try
            {
                var uri = new Uri(urlOrId);
                var s = uri.AbsoluteUri;

                // Common patterns:
                // https://drive.google.com/file/d/{id}/view?usp=sharing
                var m = Regex.Match(s, @"/d/([A-Za-z0-9_\-]+)");
                if (m.Success) return m.Groups[1].Value;

                // https://drive.google.com/open?id={id}
                m = Regex.Match(s, @"[?&]id=([A-Za-z0-9_\-]+)");
                if (m.Success) return m.Groups[1].Value;

                // https://drive.google.com/uc?id={id}&export=download
                m = Regex.Match(s, @"/uc\?id=([A-Za-z0-9_\-]+)");
                if (m.Success) return m.Groups[1].Value;
            }
            catch
            {
                // ignore
            }

            return null;
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
                _ when contentType.Contains("image") && contentType.Contains("png") => "png",
                _ => string.Empty
            };
        }
    }
}
