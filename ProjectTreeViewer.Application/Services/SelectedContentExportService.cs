using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProjectTreeViewer;

public sealed class SelectedContentExportService
{
    public string Build(IEnumerable<string> filePaths)
    {
        var files = filePaths
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (files.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        bool first = true;

        foreach (var file in files)
        {
            if (!first)
            {
                sb.AppendLine();
                sb.AppendLine();
            }

            first = false;

            sb.AppendLine($"{file}:");
            sb.AppendLine();

            try
            {
                sb.Append(ReadText(file));
            }
            catch (Exception ex)
            {
                sb.AppendLine($"[Не удалось прочитать файл: {ex.Message}]");
            }
        }

        return sb.ToString();
    }

    private static string ReadText(string file)
    {
        using var reader = new StreamReader(file, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }
}