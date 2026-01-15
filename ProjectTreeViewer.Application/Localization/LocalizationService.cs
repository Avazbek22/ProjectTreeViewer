using System;
using System.Globalization;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer;

public sealed class LocalizationService
{
    private readonly ILocalizationCatalog _catalog;

    public AppLanguage CurrentLanguage { get; private set; }

    public event EventHandler? LanguageChanged;

    public LocalizationService(AppLanguage initialLanguage)
        : this(initialLanguage, CreateDefaultCatalog())
    {
    }

    public LocalizationService(AppLanguage initialLanguage, ILocalizationCatalog catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        CurrentLanguage = initialLanguage;
    }

    public string this[string key]
    {
        get
        {
            var dict = _catalog.Get(CurrentLanguage);
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

    private static ILocalizationCatalog CreateDefaultCatalog()
    {
        // Default: читаем из ProjectTreeViewer.Assets как embedded ресурсы
        var store = AssetsResourceStoreFactory.CreateEmbeddedAssetsStore();
        return new ProjectTreeViewer.Infrastructure.Localization.ResourceLocalizationCatalog(store);
    }
}