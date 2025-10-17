using M_SAVA_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Utils
{
    public static class FileExtensionUtils
    {
        public static string GetFileExtension(SavedFileReferenceDB db)
        {
            return db.FileExtension.ToString().TrimStart('_').ToLowerInvariant();
        }
    }
}
