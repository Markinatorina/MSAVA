using System;
using System.Collections.Generic;
using System.IO;

namespace M_SAVA_Core.Models
{
    public class SaveFileFromYouTubeDTO
    {
        public required string YouTubeUrl { get; set; }
        public required Guid AccessGroupId { get; set; }
        public List<string>? Tags { get; set; } = null!;
        public List<string>? Categories { get; set; } = null!;
        public string? Description { get; set; } = null!;
        public bool PublicViewing { get; set; } = false;
        public bool PublicDownload { get; set; } = false;
        public bool DownloadVideo { get; set; } = true;
        public bool DownloadAudio { get; set; } = true;
        public string? VideoQuality { get; set; } = null!; // e.g. "1080p", "720p", "480p"
        public string? AudioQuality { get; set; } = null!; // e.g. "128kbps"
    }
}
