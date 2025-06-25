using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Models
{
    public class SavedBytesDB
    {
        public required byte[] FileHash { get; set; }
        public required FileExtensionType FileExtension { get; set; }
        public required byte[] Content { get; set; }
    }
}
