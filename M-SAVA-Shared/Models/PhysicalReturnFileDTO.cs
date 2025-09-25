using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_Shared.Models
{
    public class PhysicalReturnFileDTO
    {
        public required string FilePath { get; set; }
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
    }
}
