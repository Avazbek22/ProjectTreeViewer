using System.Collections.Generic;

namespace ProjectTreeViewer.Kernel.Abstractions;

public interface ILocalizationCatalog
{
    IReadOnlyDictionary<string, string> Get(AppLanguage language);
}