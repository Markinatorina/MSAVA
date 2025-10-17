using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Models
{
    public class SavedFileDataDB : IIdentifiableDB
    {
        public Guid Id { get; set; }
        public SavedFileReferenceDB? FileReference { get; set; }
        public required Guid FileReferenceId { get; set; }
        public required ulong SizeInBytes { get; set; }
        public required string Checksum { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string MimeType { get; set; }
        public required string FileExtension { get; set; }
        public required string[] Tags { get; set; }
        public required string[] Categories { get; set; }
        public required JsonDocument Metadata { get; set; }

        public bool PublicViewing { get; set; } = false;
        public uint DownloadCount { get; set; } = 0;

        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
        public required Guid OwnerId { get; set; }
        public UserDB? Owner { get; set; }

        public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
        public required Guid LastModifiedById { get; set; }
        public UserDB? LastModifiedBy { get; set; }
    }
}