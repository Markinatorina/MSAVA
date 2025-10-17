using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_INF.Models
{
    public class SavedFileMetaJSON
    {
        public Guid RefId { get; set; }
        public required bool PublicDownload { get; set; } = false;
        public required Guid AccessGroupId { get; set; }
    }
}
