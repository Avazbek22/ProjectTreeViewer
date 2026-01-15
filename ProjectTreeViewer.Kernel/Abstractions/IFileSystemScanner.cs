using System.Collections.Generic;

namespace ProjectTreeViewer.Kernel.Abstractions;

public interface IFileSystemScanner
{
    bool CanReadRoot(string rootPath);

    ScanResult<HashSet<string>> GetExtensions(
        string rootPath,
        bool ignoreBin,
        bool ignoreObj,
        bool ignoreDot);

    ScanResult<List<string>> GetRootFolderNames(
        string rootPath,
        bool ignoreBin,
        bool ignoreObj,
        bool ignoreDot);
}