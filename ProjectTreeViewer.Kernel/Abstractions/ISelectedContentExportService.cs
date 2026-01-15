using System.Collections.Generic;

namespace ProjectTreeViewer.Kernel.Abstractions;

public interface ISelectedContentExportService
{
    string Build(IEnumerable<string> filePaths);
}