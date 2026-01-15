using System;
using System.Collections.Generic;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer;

public sealed class RootScanService
{
    private readonly IFileSystemScanner _scanner;

    public RootScanService(IFileSystemScanner scanner)
    {
        _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
    }

    public RootScanData Scan(string rootPath, bool ignoreBin, bool ignoreObj, bool ignoreDot)
    {
        var exts = _scanner.GetExtensions(rootPath, ignoreBin, ignoreObj, ignoreDot);
        var folders = _scanner.GetRootFolderNames(rootPath, ignoreBin, ignoreObj, ignoreDot);

        var rootDenied = exts.RootAccessDenied || folders.RootAccessDenied;
        var hadDenied = exts.HadAccessDenied || folders.HadAccessDenied;

        return new RootScanData(
            RootFolderNames: folders.Value,
            Extensions: exts.Value,
            RootAccessDenied: rootDenied,
            HadAccessDenied: hadDenied);
    }
}