using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_Core.Models
{
    public class FetchFileFromOneDriveDTO
    {
        public string? FileUrl { get; set; }

        public Guid AccessGroupId { get; set; }
        public List<string>? Tags { get; set; }
        public List<string>? Categories { get; set; }
        public string? Description { get; set; }
        public bool PublicViewing { get; set; }
        public bool PublicDownload { get; set; }
    }
}
