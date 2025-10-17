using System;
using System.Collections.Generic;

namespace M_SAVA_Shared.Models
{
    public class SaveFileFromFetchDTO
    {
        public required string FileName { get; set; }
        public required string FileExtension { get; set; }
        public required string TempFilePath { get; set; }
        public required Guid AccessGroupId { get; set; }
        public List<string>? Tags { get; set; } = null!;
        public List<string>? Categories { get; set; } = null!;
        public string? Description { get; set; } = null!;
        public bool PublicViewing { get; set; } = false;
        public bool PublicDownload { get; set; } = false;
    }
}
