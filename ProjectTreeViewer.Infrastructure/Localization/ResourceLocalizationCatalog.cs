using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer.Infrastructure.Localization;

public sealed class ResourceLocalizationCatalog : ILocalizationCatalog
{
	private readonly IResourceStore _store;

	private readonly object _sync = new();
	private readonly Dictionary<AppLanguage, IReadOnlyDictionary<string, string>> _cache = new();

	public ResourceLocalizationCatalog(IResourceStore store)
	{
		_store = store ?? throw new ArgumentNullException(nameof(store));
	}

	public IReadOnlyDictionary<string, string> Get(AppLanguage lang)
	{
		lock (_sync)
		{
			if (_cache.TryGetValue(lang, out var cached))
				return cached;

			var dict = Load(lang);

			if (dict.Count == 0 && lang != AppLanguage.En)
				dict = Load(AppLanguage.En);

			_cache[lang] = dict;
			return dict;
		}
	}

	private IReadOnlyDictionary<string, string> Load(AppLanguage lang)
	{
		var code = LanguageToCode(lang);
		var resourceId = $"Localization/{code}.json";

		if (!_store.TryOpenRead(resourceId, out var stream))
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
