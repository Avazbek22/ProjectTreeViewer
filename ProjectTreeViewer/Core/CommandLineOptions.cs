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
				lang = v.ToLowerInvariant() switch
				{
					"ru" => AppLanguage.Ru,
					"en" => AppLanguage.En,
					"uz" => AppLanguage.Uz,
					"tg" => AppLanguage.Tg,
					"kk" => AppLanguage.Kk,
					"fr" => AppLanguage.Fr,
					"de" => AppLanguage.De,
					"it" => AppLanguage.It,
					_ => null
				};
				continue;
			}

			if (a.Equals("--elevationAttempted", StringComparison.OrdinalIgnoreCase))
			{
				elevationAttempted = true;
			}
		}

		return new CommandLineOptions(path, lang, elevationAttempted);
	}

	public CommandLineOptions WithElevationAttempted() => this with { ElevationAttempted = true };

	public string ToArguments()
	{
		var parts = new System.Collections.Generic.List<string>();

		if (!string.IsNullOrWhiteSpace(Path))
		{
			parts.Add("--path");
			parts.Add(Quote(Path!));
		}

		if (Language is not null)
		{
			parts.Add("--lang");
			parts.Add(LanguageToCode(Language.Value));
		}

		if (ElevationAttempted)
			parts.Add("--elevationAttempted");

		return string.Join(" ", parts);
	}

	private static string LanguageToCode(AppLanguage language) => language switch
	{
		AppLanguage.Ru => "ru",
		AppLanguage.En => "en",
		AppLanguage.Uz => "uz",
		AppLanguage.Tg => "tg",
		AppLanguage.Kk => "kk",
		AppLanguage.Fr => "fr",
		AppLanguage.De => "de",
		AppLanguage.It => "it",
		_ => "en"
	};

	private static string Quote(string value)
	{
		if (string.IsNullOrEmpty(value)) return "\"\"";
		bool needsQuotes = value.Any(ch => char.IsWhiteSpace(ch) || ch == '"');

		if (!needsQuotes) return value;

		return "\"" + value.Replace("\"", "\\\"") + "\"";
	}
}
