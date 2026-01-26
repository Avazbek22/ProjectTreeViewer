using System.Text;

namespace DevProjex.Application.Services;

public sealed class SelectedContentExportService
{
	private const string ClipboardBlankLine = "\u00A0"; // NBSP: looks empty but won't collapse on paste

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
		bool anyWritten = false;

		foreach (var file in files)
		{
			if (!TryReadFileTextForClipboard(file, out var text))
				continue;

			if (anyWritten)
			{
				AppendClipboardBlankLine(sb);
				AppendClipboardBlankLine(sb);
			}

			anyWritten = true;

			sb.AppendLine($"{file}:");
			AppendClipboardBlankLine(sb);
			sb.AppendLine(text);
		}

		return anyWritten ? sb.ToString().TrimEnd('\r', '\n') : string.Empty;
	}

	private bool TryReadFileTextForClipboard(string path, out string text)
	{
		text = string.Empty;

		try
		{
			if (!File.Exists(path))
				return false;

			var fi = new FileInfo(path);
			if (fi.Length == 0)
				return false;

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
		catch (Exception)
		{
			return false;
		}
	}

	private static void AppendClipboardBlankLine(StringBuilder sb) => sb.AppendLine(ClipboardBlankLine);
}
