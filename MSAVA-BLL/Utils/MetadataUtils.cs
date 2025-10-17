using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Xml;

namespace M_SAVA_BLL.Utils
{
    public static class MetadataUtils
    {
        // Map each extension to its extractor and content type
        private static readonly Dictionary<string, (Func<Stream, long, JsonDocument> Extractor, string ContentType)> ExtensionMap =
            new(StringComparer.OrdinalIgnoreCase)
        {
            // Documents
            { "txt", (ExtractTextFileMetadata, "text/plain") },
            { "pdf", (ExtractDefaultMetadata, "application/pdf") }, // TODO
            { "json", (ExtractDefaultMetadata, "application/json") }, // TODO
            { "docx", (ExtractOfficeDocumentMetadata, "application/vnd.openxmlformats-officedocument.wordprocessingml.document") },
            { "csv", (ExtractOfficeDocumentMetadata, "text/csv") },
            { "xlsx", (ExtractOfficeDocumentMetadata, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") },
            { "pptx", (ExtractOfficeDocumentMetadata, "application/vnd.openxmlformats-officedocument.presentationml.presentation") },
            { "rtf", (ExtractDefaultMetadata, "application/rtf") }, // TODO
            { "html", (ExtractDefaultMetadata, "text/html") }, // TODO
            { "xml", (ExtractDefaultMetadata, "application/xml") }, // TODO
            { "md", (ExtractTextFileMetadata, "text/markdown") },
            { "odt", (ExtractDefaultMetadata, "application/vnd.oasis.opendocument.text") }, // TODO
            { "ods", (ExtractDefaultMetadata, "application/vnd.oasis.opendocument.spreadsheet") }, // TODO
            { "odp", (ExtractDefaultMetadata, "application/vnd.oasis.opendocument.presentation") }, // TODO
            { "doc", (ExtractDefaultMetadata, "application/msword") }, // TODO
            { "xls", (ExtractDefaultMetadata, "application/vnd.ms-excel") }, // TODO
            { "ppt", (ExtractDefaultMetadata, "application/vnd.ms-powerpoint") }, // TODO
            { "log", (ExtractTextFileMetadata, "text/plain") },
            { "yaml", (ExtractDefaultMetadata, "application/x-yaml") }, // TODO
            { "ini", (ExtractTextFileMetadata, "text/plain") },

            // Raster images
            { "webp", (ExtractDefaultMetadata, "image/webp") }, // TODO
            { "png", (ExtractImageMetadata, "image/png") },
            { "jpeg", (ExtractImageMetadata, "image/jpeg") },
            { "jpg", (ExtractImageMetadata, "image/jpeg") },
            { "tiff", (ExtractDefaultMetadata, "image/tiff") }, // TODO
            { "bmp", (ExtractImageMetadata, "image/bmp") },
            { "gif", (ExtractImageMetadata, "image/gif") },
            { "ico", (ExtractDefaultMetadata, "image/x-icon") }, // TODO
            { "heic", (ExtractDefaultMetadata, "image/heic") }, // TODO
            { "heif", (ExtractDefaultMetadata, "image/heif") }, // TODO
            { "psd", (ExtractDefaultMetadata, "image/vnd.adobe.photoshop") }, // TODO
            { "exr", (ExtractDefaultMetadata, "image/aces") }, // TODO
            { "tga", (ExtractDefaultMetadata, "image/x-targa") }, // TODO
            { "jp2", (ExtractDefaultMetadata, "image/jp2") }, // TODO
            { "pbm", (ExtractDefaultMetadata, "image/x-portable-bitmap") }, // TODO
            { "pgm", (ExtractDefaultMetadata, "image/x-portable-graymap") }, // TODO
            { "ppm", (ExtractDefaultMetadata, "image/x-portable-pixmap") }, // TODO
            { "xbm", (ExtractDefaultMetadata, "image/x-xbitmap") }, // TODO
            { "xpm", (ExtractDefaultMetadata, "image/x-xpixmap") }, // TODO
            { "dib", (ExtractDefaultMetadata, "image/bmp") }, // TODO

            // Vector images
            { "svg", (ExtractDefaultMetadata, "image/svg+xml") }, // TODO
            { "eps", (ExtractDefaultMetadata, "application/postscript") }, // TODO
            { "ai", (ExtractDefaultMetadata, "application/postscript") }, // TODO
            { "wmf", (ExtractDefaultMetadata, "image/wmf") }, // TODO
            { "emf", (ExtractDefaultMetadata, "image/emf") }, // TODO
            { "cdr", (ExtractDefaultMetadata, "application/cdr") }, // TODO
            { "cgm", (ExtractDefaultMetadata, "image/cgm") }, // TODO
            { "dxf", (ExtractDefaultMetadata, "image/vnd.dxf") }, // TODO
            { "dwg", (ExtractDefaultMetadata, "image/vnd.dwg") }, // TODO
            { "sketch", (ExtractDefaultMetadata, "application/octet-stream") }, // TODO
            { "fig", (ExtractDefaultMetadata, "application/x-xfig") }, // TODO
            { "drw", (ExtractDefaultMetadata, "application/octet-stream") }, // TODO
            { "vsd", (ExtractDefaultMetadata, "application/vnd.visio") }, // TODO
            { "fla", (ExtractDefaultMetadata, "application/octet-stream") }, // TODO
            { "swf", (ExtractDefaultMetadata, "application/x-shockwave-flash") }, // TODO
            { "sai", (ExtractDefaultMetadata, "application/octet-stream") }, // TODO
            { "svgz", (ExtractDefaultMetadata, "image/svg+xml") }, // TODO
            { "hpgl", (ExtractDefaultMetadata, "application/vnd.hp-hpgl") }, // TODO
            { "plt", (ExtractDefaultMetadata, "application/plt") }, // TODO

            // Audio
            { "wav", (ExtractDefaultMetadata, "audio/wav") }, // TODO
            { "flac", (ExtractDefaultMetadata, "audio/flac") }, // TODO
            { "mp3", (ExtractDefaultMetadata, "audio/mpeg") }, // TODO
            { "aac", (ExtractDefaultMetadata, "audio/aac") }, // TODO
            { "ogg", (ExtractDefaultMetadata, "audio/ogg") }, // TODO
            { "m4a", (ExtractDefaultMetadata, "audio/mp4") }, // TODO
            { "wma", (ExtractDefaultMetadata, "audio/x-ms-wma") }, // TODO
            { "aiff", (ExtractDefaultMetadata, "audio/aiff") }, // TODO
            { "opus", (ExtractDefaultMetadata, "audio/opus") }, // TODO
            { "amr", (ExtractDefaultMetadata, "audio/amr") }, // TODO
            { "ape", (ExtractDefaultMetadata, "audio/ape") }, // TODO
            { "mpc", (ExtractDefaultMetadata, "audio/mpc") }, // TODO
            { "wv", (ExtractDefaultMetadata, "audio/wavpack") }, // TODO
            { "tta", (ExtractDefaultMetadata, "audio/tta") }, // TODO
            { "dsf", (ExtractDefaultMetadata, "audio/dsf") }, // TODO
            { "ra", (ExtractDefaultMetadata, "audio/x-pn-realaudio") }, // TODO
            { "gsm", (ExtractDefaultMetadata, "audio/gsm") }, // TODO
            { "au", (ExtractDefaultMetadata, "audio/basic") }, // TODO
            { "vox", (ExtractDefaultMetadata, "audio/voxware") }, // TODO

            { "webm", (ExtractDefaultMetadata, "video/webm") }, // TODO
            { "avi", (ExtractDefaultMetadata, "video/x-msvideo") }, // TODO
            { "mp4", (ExtractDefaultMetadata, "video/mp4") }, // TODO
            { "ogv", (ExtractDefaultMetadata, "video/ogg") }, // TODO
            { "mkv", (ExtractDefaultMetadata, "video/x-matroska") }, // TODO
            { "mov", (ExtractDefaultMetadata, "video/quicktime") }, // TODO
            { "wmv", (ExtractDefaultMetadata, "video/x-ms-wmv") }, // TODO
            { "flv", (ExtractDefaultMetadata, "video/x-flv") }, // TODO
            { "m4v", (ExtractDefaultMetadata, "video/x-m4v") }, // TODO
            { "mpg", (ExtractDefaultMetadata, "video/mpeg") }, // TODO
            { "mpeg", (ExtractDefaultMetadata, "video/mpeg") }, // TODO
            { "3gp", (ExtractDefaultMetadata, "video/3gpp") }, // TODO
            { "3g2", (ExtractDefaultMetadata, "video/3gpp2") }, // TODO
            { "asf", (ExtractDefaultMetadata, "video/x-ms-asf") }, // TODO
            { "rm", (ExtractDefaultMetadata, "application/vnd.rn-realmedia") }, // TODO
            { "vob", (ExtractDefaultMetadata, "video/dvd") }, // TODO
            { "ts", (ExtractDefaultMetadata, "video/mp2t") }, // TODO
            { "mts", (ExtractDefaultMetadata, "video/mp2t") }, // TODO
            { "m2ts", (ExtractDefaultMetadata, "video/mp2t") }, // TODO
            { "f4v", (ExtractDefaultMetadata, "video/x-f4v") }, // TODO
        };

        public static JsonDocument ExtractMetadataFromFileStream(Stream fileStream, string extension, long size = -1)
        {
            if (ExtensionMap.TryGetValue(extension.ToLowerInvariant(), out var entry))
            {
                return entry.Extractor(fileStream, size);
            }
            return ExtractDefaultMetadata(fileStream, size);
        }

        public static string GetContentType(string extension)
        {
            if (ExtensionMap.TryGetValue(extension.ToLowerInvariant(), out var entry))
            {
                return entry.ContentType;
            }
            return "application/octet-stream";
        }

        // Extractors
        private static JsonDocument ExtractImageMetadata(Stream fileStream, long size)
        {
            using var image = Image.FromStream(fileStream, useEmbeddedColorManagement: false, validateImageData: false);
            var metadata = new
            {
                Type = "Image",
                Format = image.RawFormat.ToString(),
                Width = image.Width,
                Height = image.Height,
                HorizontalResolution = image.HorizontalResolution,
                VerticalResolution = image.VerticalResolution,
                PixelFormat = image.PixelFormat.ToString(),
                Size = size
            };
            return JsonDocument.Parse(JsonSerializer.Serialize(metadata));
        }

        private static JsonDocument ExtractOfficeDocumentMetadata(Stream fileStream, long size)
        {
            using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read, leaveOpen: true);
            var entry = archive.GetEntry("docProps/core.xml");
            if (entry != null)
            {
                using var xmlStream = entry.Open();
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlStream);
                var title = xmlDoc.SelectSingleNode("//dc:title")?.InnerText;
                var author = xmlDoc.SelectSingleNode("//dc:creator")?.InnerText;
                var metadata = new { Type = "OfficeDocument", Title = title, Author = author, Size = size };
                return JsonDocument.Parse(JsonSerializer.Serialize(metadata));
            }
            return JsonDocument.Parse("{}", new JsonDocumentOptions());
        }

        private static JsonDocument ExtractTextFileMetadata(Stream fileStream, long size)
        {
            using var reader = new StreamReader(fileStream);
            var content = reader.ReadToEnd();
            var lineCount = content.Split('\n').Length;
            var metadata = new { Type = "Text", LineCount = lineCount, Size = size };
            return JsonDocument.Parse(JsonSerializer.Serialize(metadata));
        }

        private static JsonDocument ExtractDefaultMetadata(Stream fileStream, long size)
        {
            var metadata = new { Type = "Unknown", Size = size };
            return JsonDocument.Parse(JsonSerializer.Serialize(metadata));
        }
    }
}
