using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_Core.Models
{
    public class StreamReturnFileDTO
    {
        public Guid Id { get; set; }
        public required string FileName { get; set; }
        public required string FileExtension { get; set; }
        public required Stream FileStream { get; set; }
    }
}
