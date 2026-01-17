using System.Globalization;

namespace ProjectTreeViewer.Kernel.Models;

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
			var arg = args[i];

			if (arg.Equals("--path", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
			{
				path = args[++i];
				continue;
			}

			if (arg.Equals("--lang", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
			{
				var value = args[++i];
				lang = ParseLanguage(value);
				continue;
			}

			if (arg.Equals("--elevationAttempted", StringComparison.OrdinalIgnoreCase))
				elevationAttempted = true;
		}

		return new CommandLineOptions(path, lang, elevationAttempted);
	}

	public CommandLineOptions WithElevationAttempted() => this with { ElevationAttempted = true };

	public string ToArguments()
	{
		var parts = new List<string>();

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

	public static AppLanguage? ParseLanguage(string code)
	{
		if (string.IsNullOrWhiteSpace(code)) return null;

		return code.Trim().ToLowerInvariant() switch
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
	}

	public static string LanguageToCode(AppLanguage language) => language switch
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

	public static AppLanguage DetectSystemLanguage()
	{
		var code = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
		return code switch
		{
			"ru" => AppLanguage.Ru,
			"uz" => AppLanguage.Uz,
			"tg" => AppLanguage.Tg,
			"kk" => AppLanguage.Kk,
			"fr" => AppLanguage.Fr,
			"de" => AppLanguage.De,
			"it" => AppLanguage.It,
			_ => AppLanguage.En
		};
	}

	private static string Quote(string value)
	{
		if (string.IsNullOrEmpty(value)) return "\"\"";
		bool needsQuotes = value.Any(ch => char.IsWhiteSpace(ch) || ch == '"');

		if (!needsQuotes) return value;

		return "\"" + value.Replace("\"", "\\\"") + "\"";
	}
}
