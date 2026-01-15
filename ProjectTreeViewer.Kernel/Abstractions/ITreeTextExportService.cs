using System.Collections.Generic;

namespace ProjectTreeViewer.Kernel.Abstractions;

public interface ITreeTextExportService
{
    string BuildFullTree(string rootPath, FileSystemNode root);

    string BuildSelectedTree(string rootPath, FileSystemNode root, IEnumerable<string> selectedPaths);
}