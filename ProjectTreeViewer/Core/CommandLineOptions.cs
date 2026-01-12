using System;
using System.Linq;

namespace ProjectTreeViewer;

public sealed record CommandLineOptions(
	string? Path,
	AppLanguage? Language,
	bool ElevationAttempted)
{
	public static CommandLineOptions Empty { get; } = new(null, null, false);

	public static CommandLineOptions Parse(string[] args)
	{
		if (args.Length == 0) return Empty;

		string? path = null;
		AppLanguage? lang = null;
		bool elevationAttempted = false;

		for (int i = 0; i < args.Length; i++)
		{
			var a = args[i];

			if (a.Equals("--path", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
			{
				path = args[++i];
				continue;
			}

			if (a.Equals("--lang", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
			{
				var v = args[++i];
				lang = v.Equals("ru", StringComparison.OrdinalIgnoreCase) ? AppLanguage.Ru :
					v.Equals("en", StringComparison.OrdinalIgnoreCase) ? AppLanguage.En :
					null;
				continue;
			}

			if (a.Equals("--elevationAttempted", StringComparison.OrdinalIgnoreCase))
			{
				elevationAttempted = true;
			}
		}

		return new CommandLineOptions(path, lang, elevationAttempted);
	}

	public string ToArguments()
	{
		// Важно: UseShellExecute=true (runas) => формируем строку аргументов сами.
		var parts = new System.Collections.Generic.List<string>();

		if (!string.IsNullOrWhiteSpace(Path))
		{
			parts.Add("--path");
			parts.Add(Quote(Path!));
		}

		if (Language is not null)
		{
			parts.Add("--lang");
			parts.Add(Language == AppLanguage.Ru ? "ru" : "en");
		}

		// Чтобы не уйти в бесконечный цикл перезапусков.
		if (ElevationAttempted)
		{
			parts.Add("--elevationAttempted");
		}

		return string.Join(" ", parts);
	}

	private static string Quote(string value)
	{
		if (string.IsNullOrEmpty(value)) return "\"\"";
		bool needsQuotes = value.Any(ch => char.IsWhiteSpace(ch) || ch == '"');

		if (!needsQuotes) return value;

		// Достаточно для типичных путей Windows
		return "\"" + value.Replace("\"", "\\\"") + "\"";
	}
}
