using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProjectTreeViewer
{
    public sealed class ContentReadService
    {
        private static readonly HashSet<string> ContentExcludedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".dll", ".exe", ".msi",

            ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".tif", ".tiff", ".webp", ".svg", ".ico",

            ".mp4", ".mkv", ".mov", ".avi", ".webm", ".wmv", ".m4v", ".flv",

            ".mp3", ".wav", ".flac", ".aac", ".ogg", ".m4a", ".wma", ".opus", ".aiff",

            ".pdf",
            ".doc", ".docx", ".dot", ".dotx",
            ".xls", ".xlsx", ".xlsm", ".xlt", ".xltx",
            ".ppt", ".pptx", ".pps", ".ppsx",

            ".mdb", ".accdb",

            ".zip", ".7z", ".rar", ".tar", ".gz",

            ".pdb"
        };

        public bool TryReadTextForClipboard(string path, out string text)
        {
            text = string.Empty;

            try
            {
                if (!File.Exists(path))
                    return false;

                var ext = Path.GetExtension(path);
                if (!string.IsNullOrWhiteSpace(ext) && ContentExcludedExtensions.Contains(ext))
                    return false;

                var fi = new FileInfo(path);
                if (fi.Length == 0)
                    return false;

                // Быстрый binary sniff: первые 8 KB, ищем NUL-байты
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    int toRead = (int)Math.Min(8192, fs.Length);
                    var buffer = new byte[toRead];
                    int read = fs.Read(buffer, 0, toRead);

                    for (int i = 0; i < read; i++)
                    {
                        if (buffer[i] == 0)
                            return false;
                    }
                }

                string raw;
                using (var reader = new StreamReader(path, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                    raw = reader.ReadToEnd();

                if (string.IsNullOrWhiteSpace(raw))
                    return false;

                if (raw.IndexOf('\0') >= 0)
                    return false;

                text = raw.TrimEnd('\r', '\n');
                return !string.IsNullOrWhiteSpace(text);
            }
            catch
            {
                return false;
            }
        }
    }
}
