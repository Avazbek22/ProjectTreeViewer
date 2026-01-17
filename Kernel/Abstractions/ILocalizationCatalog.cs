using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Kernel.Abstractions;

public interface ILocalizationCatalog
{
	IReadOnlyDictionary<string, string> Get(AppLanguage language);
}
