using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M_SAVA_Core.Models;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using M_SAVA_BLL.Loggers;

namespace M_SAVA_BLL.Services
{
    public class FetchFileService
    {
        private readonly SaveFileService _saveFileService;
        private readonly ServiceLogger _serviceLogger;
        public FetchFileService(SaveFileService saveFileService, ServiceLogger serviceLogger)
        {
            _saveFileService = saveFileService;
            _serviceLogger = serviceLogger ?? throw new ArgumentNullException(nameof(serviceLogger));
        }

        public async Task<Guid> CreateFileFromYouTubeAsync(SaveFileFromYouTubeDTO dto, CancellationToken cancellationToken = default)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.YouTubeUrl))
                throw new ArgumentException("YouTubeUrl must be provided.", nameof(dto));
            if (dto.AccessGroupId == Guid.Empty)
                throw new ArgumentException("AccessGroupId must be provided.", nameof(dto));

            var youtube = new YoutubeClient();
            var videoId = VideoId.Parse(dto.YouTubeUrl);
            var video = await youtube.Videos.GetAsync(videoId, cancellationToken);
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId, cancellationToken);
            var muxedStreams = streamManifest.GetMuxedStreams().ToList();
            var videoOnlyStreams = streamManifest.GetVideoOnlyStreams().ToList();
            var audioOnlyStreams = streamManifest.GetAudioOnlyStreams().ToList();

            string fileName = string.IsNullOrWhiteSpace(video.Title) ? "YouTube Video" : video.Title;
            string fileExtension = "mp4";
            string tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".mp4");

            if (dto.DownloadVideo && dto.DownloadAudio)
            {
                // Try muxed first
                MuxedStreamInfo? streamInfo = null;
                if (!string.IsNullOrWhiteSpace(dto.VideoQuality))
                {
                    streamInfo = muxedStreams
                        .Where(s => s.VideoQuality.Label.Equals(dto.VideoQuality, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(s => s.Bitrate)
                        .FirstOrDefault();
                }
                if (streamInfo == null)
                {
                    streamInfo = muxedStreams
                        .OrderByDescending(s => s.VideoQuality.MaxHeight)
                        .ThenByDescending(s => s.Bitrate)
                        .FirstOrDefault();
                }
                if (streamInfo != null)
                {
                    fileExtension = streamInfo.Container.Name;
                    using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await youtube.Videos.Streams.CopyToAsync(streamInfo, fileStream, null, cancellationToken);
                    }
                }
                else
                {
                    // No muxed stream: download video-only and audio-only, then mux with ffmpeg
                    IVideoStreamInfo? videoStream = null;
                    IAudioStreamInfo? audioStream = null;

                    if (!string.IsNullOrWhiteSpace(dto.VideoQuality))
                    {
                        videoStream = videoOnlyStreams
                            .Where(s => s.VideoQuality.Label.Equals(dto.VideoQuality, StringComparison.OrdinalIgnoreCase))
                            .OrderByDescending(s => s.Bitrate)
                            .FirstOrDefault();
                    }
                    if (videoStream == null)
                    {
                        videoStream = videoOnlyStreams
                            .OrderByDescending(s => s.VideoQuality.MaxHeight)
                            .ThenByDescending(s => s.Bitrate)
                            .FirstOrDefault();
                    }

                    if (!string.IsNullOrWhiteSpace(dto.AudioQuality))
                    {
                        audioStream = audioOnlyStreams
                            .Where(s => (s.Bitrate.KiloBitsPerSecond + "kbps").Equals(dto.AudioQuality, StringComparison.OrdinalIgnoreCase))
                            .OrderByDescending(s => s.Bitrate)
                            .FirstOrDefault();
                    }
                    if (audioStream == null)
                    {
                        audioStream = audioOnlyStreams
                            .OrderByDescending(s => s.Bitrate)
                            .FirstOrDefault();
                    }

                    if (videoStream == null || audioStream == null)
                    {
                        string availableVideo = videoOnlyStreams.Any() ? string.Join(", ", videoOnlyStreams.Select(s => s.VideoQuality.Label)) : "(none)";
                        string availableAudio = audioOnlyStreams.Any() ? string.Join(", ", audioOnlyStreams.Select(s => s.Bitrate.KiloBitsPerSecond + "kbps")) : "(none)";
                        throw new InvalidOperationException($"No suitable muxed stream, and could not find both video and audio-only streams. Video qualities: {availableVideo}. Audio qualities: {availableAudio}.");
                    }

                    // Correct container extensions
                    string videoExt = videoStream.Container.Name;
                    string audioExt = audioStream.Container.Name;

                    string videoTemp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + "." + videoExt);
                    string audioTemp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + "." + audioExt);
                    string muxedTemp = tempFilePath;
                    var ffmpegPath = "ffmpeg";

                    try
                    {
                        _serviceLogger.LogInformation($"Downloading video to {videoTemp} and audio to {audioTemp}");

                        using (var vfs = new FileStream(videoTemp, FileMode.Create, FileAccess.Write, FileShare.None))
                            await youtube.Videos.Streams.CopyToAsync(videoStream, vfs, null, cancellationToken);

                        using (var afs = new FileStream(audioTemp, FileMode.Create, FileAccess.Write, FileShare.None))
                            await youtube.Videos.Streams.CopyToAsync(audioStream, afs, null, cancellationToken);

                        string args = $"-y -i \"{videoTemp}\" -i \"{audioTemp}\" -c:v copy -c:a aac -shortest \"{muxedTemp}\"";
                        _serviceLogger.LogInformation($"Starting FFmpeg mux: {args}");

                        var psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = ffmpegPath,
                            Arguments = args,
                            RedirectStandardError = true, 
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        using (var process = System.Diagnostics.Process.Start(psi))
                        {
                            if (process == null)
                                throw new InvalidOperationException("Failed to start FFmpeg process.");

                            var stderrTask = process.StandardError.ReadToEndAsync();

                            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                            timeoutCts.CancelAfter(TimeSpan.FromSeconds(15)); 

                            try
                            {
                                await process.WaitForExitAsync(timeoutCts.Token);
                            }
                            catch (OperationCanceledException)
                            {
                                try { if (!process.HasExited) process.Kill(); } catch { }
                                _serviceLogger.WriteLog(500, $"FFmpeg process timed out after 15s. Args: {args}", null);
                                throw new TimeoutException("FFmpeg process exceeded 15 seconds and was terminated.");
                            }

                            string errorOutput = await stderrTask;

                            if (process.ExitCode != 0)
                            {
                                _serviceLogger.WriteLog(500, $"FFmpeg failed. Args: {args} Error: {errorOutput}", null);
                                throw new InvalidOperationException($"FFmpeg failed to mux video and audio: {errorOutput}");
                            }
                            else
                            {
                                _serviceLogger.LogInformation($"FFmpeg mux completed successfully. Output: {muxedTemp}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _serviceLogger.WriteLog(500, $"Exception during FFmpeg mux: {ex.Message}", null);
                        throw;
                    }
                    finally
                    {
                        try { if (File.Exists(videoTemp)) File.Delete(videoTemp); } catch { }
                        try { if (File.Exists(audioTemp)) File.Delete(audioTemp); } catch { }
                    }
                }
            }
            else if (dto.DownloadVideo)
            {
                // Video only
                IVideoStreamInfo? streamInfo = null;
                if (!string.IsNullOrWhiteSpace(dto.VideoQuality))
                {
                    streamInfo = videoOnlyStreams
                        .Where(s => s.VideoQuality.Label.Equals(dto.VideoQuality, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(s => s.Bitrate)
                        .FirstOrDefault();
                }
                if (streamInfo == null)
                {
                    streamInfo = videoOnlyStreams
                        .OrderByDescending(s => s.VideoQuality.MaxHeight)
                        .ThenByDescending(s => s.Bitrate)
                        .FirstOrDefault();
                }
                if (streamInfo == null)
                {
                    string availableVideo = videoOnlyStreams.Any() ? string.Join(", ", videoOnlyStreams.Select(s => s.VideoQuality.Label)) : "(none)";
                    throw new InvalidOperationException($"No suitable video-only stream found. Requested quality: '{dto.VideoQuality ?? "(not specified)"}'. Available video-only qualities: {availableVideo}.");
                }
                fileExtension = streamInfo.Container.Name;
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await youtube.Videos.Streams.CopyToAsync(streamInfo, fileStream, null, cancellationToken);
                }
            }
            else if (dto.DownloadAudio)
            {
                // Audio only
                IAudioStreamInfo? streamInfo = null;
                if (!string.IsNullOrWhiteSpace(dto.AudioQuality))
                {
                    streamInfo = audioOnlyStreams
                        .Where(s => (s.Bitrate.KiloBitsPerSecond + "kbps").Equals(dto.AudioQuality, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(s => s.Bitrate)
                        .FirstOrDefault();
                }
                if (streamInfo == null)
                {
                    streamInfo = audioOnlyStreams
                        .OrderByDescending(s => s.Bitrate)
                        .FirstOrDefault();
                }
                if (streamInfo == null)
                {
                    string availableAudio = audioOnlyStreams.Any() ? string.Join(", ", audioOnlyStreams.Select(s => s.Bitrate.KiloBitsPerSecond + "kbps")) : "(none)";
                    throw new InvalidOperationException($"No suitable audio-only stream found. Requested quality: '{dto.AudioQuality ?? "(not specified)"}'. Available audio-only qualities: {availableAudio}.");
                }
                fileExtension = streamInfo.Container.Name;
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await youtube.Videos.Streams.CopyToAsync(streamInfo, fileStream, null, cancellationToken);
                }
            }
            else
            {
                throw new InvalidOperationException("Neither DownloadVideo nor DownloadAudio is set to true. At least one must be selected.");
            }

            var fetchDto = new SaveFileFromFetchDTO
            {
                FileName = fileName,
                FileExtension = fileExtension,
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
    }
}
