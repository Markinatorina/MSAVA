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

namespace M_SAVA_BLL.Services
{
    public class FetchFileService
    {
        private readonly SaveFileService _saveFileService;
        public FetchFileService(SaveFileService saveFileService)
        {
            _saveFileService = saveFileService;
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
            string fileExtension = "";
            string tempFilePath = Path.GetTempFileName();

            if (dto.DownloadVideo && dto.DownloadAudio)
            {
                // Try muxed
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
                if (streamInfo == null)
                {
                    string availableMuxed = muxedStreams.Any() ? string.Join(", ", muxedStreams.Select(s => s.VideoQuality.Label)) : "(no muxed streams available)";
                    throw new InvalidOperationException($"No suitable muxed (audio+video) stream found. Requested quality: '{dto.VideoQuality ?? "(not specified)"}'. Available muxed qualities: {availableMuxed}. This may be due to video restrictions, region locks, or the video only having separate audio/video streams.");
                }
                fileExtension = streamInfo.Container.Name;
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await youtube.Videos.Streams.CopyToAsync(streamInfo, fileStream, null, cancellationToken);
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
