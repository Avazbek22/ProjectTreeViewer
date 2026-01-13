using System;
using System.Globalization;

namespace ProjectTreeViewer;

public sealed class LocalizationService
{
	public AppLanguage CurrentLanguage { get; private set; }

	public event EventHandler? LanguageChanged;

	public LocalizationService(AppLanguage initialLanguage)
	{
		CurrentLanguage = initialLanguage;
	}

	public string this[string key]
	{
		get
		{
			var dict = LocalizationCatalog.Get(CurrentLanguage);
			return dict.TryGetValue(key, out var value) ? value : $"[[{key}]]";
		}
	}

	public string Format(string key, params object[] args) => string.Format(this[key], args);

	public void SetLanguage(AppLanguage language)
	{
		if (CurrentLanguage == language) return;

		CurrentLanguage = language;
		LanguageChanged?.Invoke(this, EventArgs.Empty);
	}

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
}
