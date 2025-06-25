using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Models
{
    public class SavedFileDB : IIdentifiableDB
    {
        public Guid Id { get; set; }
        public required byte[] FileHash { get; set; }
        public required FileExtensionType FileExtension { get; set; }
        public UserDB Uploader { get; set; }
    }
}
