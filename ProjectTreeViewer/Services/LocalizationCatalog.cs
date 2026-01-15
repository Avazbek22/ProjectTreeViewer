using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ProjectTreeViewer.Assets;
using ProjectTreeViewer.Infrastructure.ResourceStore;

namespace ProjectTreeViewer;

public static class LocalizationCatalog
{
	private static readonly object Sync = new();
	private static readonly Dictionary<AppLanguage, IReadOnlyDictionary<string, string>> Cache = new();

	private static readonly EmbeddedResourceStore Store =
		new(AssetsAssembly.Assembly, baseNamespace: "ProjectTreeViewer.Assets");

	public static IReadOnlyDictionary<string, string> Get(AppLanguage lang)
	{
		lock (Sync)
		{
			if (Cache.TryGetValue(lang, out var cached))
				return cached;

			var dict = LoadFromJson(lang);

			// fallback на en, если нужного файла нет/битый
			if (dict.Count == 0 && lang != AppLanguage.En)
				dict = LoadFromJson(AppLanguage.En);

			Cache[lang] = dict;
			return dict;
		}
	}

	private static IReadOnlyDictionary<string, string> LoadFromJson(AppLanguage lang)
	{
		var code = LanguageToCode(lang);
		var resourceId = $"Localization/{code}.json";

		if (!Store.TryOpenRead(resourceId, out var stream))
			return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		try
		{
			using (stream)
			using (var reader = new StreamReader(stream))
			{
				var json = reader.ReadToEnd();

				var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(
					json,
					new JsonSerializerOptions
					{
						ReadCommentHandling = JsonCommentHandling.Skip,
						AllowTrailingCommas = true
					});

				return parsed is null
					? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
					: new Dictionary<string, string>(parsed, StringComparer.OrdinalIgnoreCase);
			}
		}
		catch
		{
			return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		}
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
}
