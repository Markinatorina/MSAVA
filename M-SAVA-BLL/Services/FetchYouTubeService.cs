using M_SAVA_BLL.Loggers;
using M_SAVA_Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace M_SAVA_BLL.Services
{
    public class FetchYouTubeFileService
    {
        private readonly SaveFileService _saveFileService;
        private readonly ServiceLogger _serviceLogger;

        public FetchYouTubeFileService(SaveFileService saveFileService, ServiceLogger serviceLogger)
        {
            _saveFileService = saveFileService;
            _serviceLogger = serviceLogger ?? throw new ArgumentNullException(nameof(serviceLogger));
        }

        public async Task<Guid> NoAuthFileFetch(FetchFileYouTubeDTO dto, CancellationToken cancellationToken = default)
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
            string fileExtension = "mp4"; // Final output extension for metadata
            string tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            if (dto.DownloadVideo && dto.DownloadAudio)
            {
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
                    // Separate video & audio → mux
                    var videoStream = GetBestVideoStream(videoOnlyStreams.Cast<IVideoStreamInfo>(), dto.VideoQuality);
                    var audioStream = GetBestAudioStream(audioOnlyStreams.Cast<IAudioStreamInfo>(), dto.AudioQuality);

                    string videoFormat = videoStream.Container.Name;
                    string audioFormat = audioStream.Container.Name;

                    string videoTemp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    string audioTemp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    string muxedTemp = tempFilePath;
                    var ffmpegPath = "ffmpeg";

                    try
                    {
                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        _serviceLogger.LogInformation($"Downloading video to {videoTemp}");
                        using (var vfs = new FileStream(videoTemp, FileMode.Create, FileAccess.Write, FileShare.None))
                            await youtube.Videos.Streams.CopyToAsync(videoStream, vfs, null, cancellationToken);
                        sw.Stop();
                        _serviceLogger.LogInformation($"Video download completed in {sw.Elapsed.TotalSeconds:F2} seconds");

                        sw.Restart();
                        _serviceLogger.LogInformation($"Downloading audio to {audioTemp}");
                        using (var afs = new FileStream(audioTemp, FileMode.Create, FileAccess.Write, FileShare.None))
                            await youtube.Videos.Streams.CopyToAsync(audioStream, afs, null, cancellationToken);
                        sw.Stop();
                        _serviceLogger.LogInformation($"Audio download completed in {sw.Elapsed.TotalSeconds:F2} seconds");

                        string args = $"-y -f {videoFormat} -i \"{videoTemp}\" -f {audioFormat} -i \"{audioTemp}\" -c:v copy -c:a aac -shortest -f {fileExtension} \"{muxedTemp}\"";
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

                            sw.Restart();
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
                            sw.Stop();

                            string errorOutput = await stderrTask;

                            if (process.ExitCode != 0)
                            {
                                _serviceLogger.WriteLog(500, $"FFmpeg failed. Args: {args} Error: {errorOutput}", null);
                                throw new InvalidOperationException($"FFmpeg failed to mux video and audio: {errorOutput}");
                            }
                            else
                            {
                                _serviceLogger.LogInformation($"FFmpeg mux completed successfully in {sw.Elapsed.TotalSeconds:F2} seconds. Output: {muxedTemp}");
                            }
                        }
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
                var videoStream = GetBestVideoStream(videoOnlyStreams, dto.VideoQuality);
                fileExtension = videoStream.Container.Name;
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await youtube.Videos.Streams.CopyToAsync(videoStream, fileStream, null, cancellationToken);
                }
            }
            else if (dto.DownloadAudio)
            {
                var audioStream = GetBestAudioStream(audioOnlyStreams, dto.AudioQuality);
                fileExtension = audioStream.Container.Name;
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await youtube.Videos.Streams.CopyToAsync(audioStream, fileStream, null, cancellationToken);
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

        private static IVideoStreamInfo GetBestVideoStream(IEnumerable<IVideoStreamInfo> streams, string? preferredQuality)
        {
            IVideoStreamInfo? stream = null;
            if (!string.IsNullOrWhiteSpace(preferredQuality))
            {
                stream = streams
                    .Where(s => s.VideoQuality.Label.Equals(preferredQuality, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(s => s.Bitrate)
                    .FirstOrDefault();
            }
            return stream ?? streams
                .OrderByDescending(s => s.VideoQuality.MaxHeight)
                .ThenByDescending(s => s.Bitrate)
                .FirstOrDefault()
                ?? throw new InvalidOperationException("No suitable video-only stream found.");
        }
        private static IAudioStreamInfo GetBestAudioStream(IEnumerable<IAudioStreamInfo> streams, string? preferredQuality)
        {
            IAudioStreamInfo? stream = null;
            if (!string.IsNullOrWhiteSpace(preferredQuality))
            {
                stream = streams
                    .Where(s => (s.Bitrate.KiloBitsPerSecond + "kbps").Equals(preferredQuality, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(s => s.Bitrate)
                    .FirstOrDefault();
            }
            return stream ?? streams
                .OrderByDescending(s => s.Bitrate)
                .FirstOrDefault()
                ?? throw new InvalidOperationException("No suitable audio-only stream found.");
        }
    }
}
