using System.Collections.Generic;
using ProjectTreeViewer.Infrastructure.Localization;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer;

public static class LocalizationCatalog
{
    private static readonly object Sync = new();
    private static ILocalizationCatalog? _catalog;

    public static IReadOnlyDictionary<string, string> Get(AppLanguage lang)
    {
        Ensure();
        return _catalog!.Get(lang);
    }

    private static void Ensure()
    {
        if (_catalog is not null)
            return;

        lock (Sync)
        {
            if (_catalog is not null)
                return;

            var store = AssetsResourceStoreFactory.CreateEmbeddedAssetsStore();
            _catalog = new ResourceLocalizationCatalog(store);
        }
    }
}