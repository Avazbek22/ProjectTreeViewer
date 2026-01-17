using System.Collections.Generic;

namespace ProjectTreeViewer.Kernel.Abstractions;

public interface IDefaultExtensionCatalog
{
	IReadOnlyCollection<string> GetDefaultExtensions();
}
