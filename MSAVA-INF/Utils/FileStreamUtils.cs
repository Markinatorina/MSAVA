using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_INF.Utils
{
    public static class FileStreamUtils
    {
        public static FileStreamOptions GetDefaultFileStreamOptions()
        {
            return new FileStreamOptions
            {
                Mode = FileMode.Open,
                Access = FileAccess.Read,
                Share = FileShare.Read,
                BufferSize = 81920,
                Options = FileOptions.Asynchronous
            };
        }
        public static async Task<(ulong length, byte[] hash, byte[] fileBytes, MemoryStream memoryStream)> ExtractFileStreamData(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            MemoryStream memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();
            ulong length = (ulong)fileBytes.Length;
            byte[] hash;
            using (SHA256 sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(fileBytes);
            }
            memoryStream.Position = 0;
            return (length, hash, fileBytes, memoryStream);
        }
    }
}
