using System;
using System.Collections.Generic;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer;

public sealed class TreeViewerBackend
{
    public IFileSystemScanner Scanner { get; }
    public ITreeBuilder Builder { get; }
    public ISelectedContentExportService ContentExporter { get; }

    public RootScanService RootScan { get; }

    public TreeViewerBackend(
        IFileSystemScanner scanner,
        ITreeBuilder builder,
        ISelectedContentExportService contentExporter)
    {
        Scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
        Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        ContentExporter = contentExporter ?? throw new ArgumentNullException(nameof(contentExporter));

        RootScan = new RootScanService(Scanner);
    }

    public static TreeViewerBackend CreateDefault()
    {
        IFileSystemScanner scanner = new FileSystemScanner();
        ITreeBuilder builder = new TreeBuilder();
        ISelectedContentExportService exporter = new SelectedContentExportService();

        return new TreeViewerBackend(scanner, builder, exporter);
    }

    public RootScanData ScanRoot(string rootPath, bool ignoreBin, bool ignoreObj, bool ignoreDot)
        => RootScan.Scan(rootPath, ignoreBin, ignoreObj, ignoreDot);

    public TreeBuildResult BuildTree(string rootPath, TreeFilterOptions options)
        => Builder.Build(rootPath, options);

    public string BuildSelectedContent(IEnumerable<string> filePaths)
        => ContentExporter.Build(filePaths);
}