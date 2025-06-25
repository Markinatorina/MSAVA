using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Models
{
    public class FileToSaveDTO
    {
        public Guid? Id { get; set; }
        public string? FileName { get; set; }
        public byte[]? FileHash { get; set; }
        public required string FileExtension { get; set; }
        public IFormFile? FormFile { get; set; } = null!;
        public byte[]? Bytes { get; set; } = null!;
        public Stream? Stream { get; set; } = null!;
    }
}
