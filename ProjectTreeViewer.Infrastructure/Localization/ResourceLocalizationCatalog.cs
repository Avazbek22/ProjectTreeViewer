using System;
using System.Collections.Generic;
using ProjectTreeViewer.Infrastructure.Resources;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer.Infrastructure.Localization;

public sealed class ResourceLocalizationCatalog : ILocalizationCatalog
{
	private readonly IResourceStore _store;
	private readonly string _basePath;
	private readonly AppLanguage _fallbackLanguage;

	private readonly object _sync = new();
	private readonly Dictionary<AppLanguage, IReadOnlyDictionary<string, string>> _cache = new();

	public ResourceLocalizationCatalog(
		IResourceStore store,
		string basePath = "Localization",
		AppLanguage fallbackLanguage = AppLanguage.En)
	{
		_store = store ?? throw new ArgumentNullException(nameof(store));
		_basePath = string.IsNullOrWhiteSpace(basePath) ? "Localization" : basePath;
		_fallbackLanguage = fallbackLanguage;
	}

	public IReadOnlyDictionary<string, string> Get(AppLanguage language)
	{
		lock (_sync)
		{
			if (_cache.TryGetValue(language, out var cached))
				return cached;

			var dict = Load(language);

			if (dict.Count == 0 && language != _fallbackLanguage)
				dict = Load(_fallbackLanguage);

			_cache[language] = dict;
			return dict;
		}
	}

	private IReadOnlyDictionary<string, string> Load(AppLanguage language)
	{
		var code = LanguageToCode(language);
		var resourceId = $"{_basePath}/{code}.json";

		var reader = new ResourceJsonReader(_store);

		if (!reader.TryReadJson<Dictionary<string, string>>(resourceId, out var parsed) || parsed is null)
			return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		return new Dictionary<string, string>(parsed, StringComparer.OrdinalIgnoreCase);
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
