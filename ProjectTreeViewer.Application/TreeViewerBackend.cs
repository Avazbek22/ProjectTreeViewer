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

	// Новый “кусок бэкенда”: ресурсы/иконки/локализация
	public IIconPackService Icons { get; }
	public ILocalizationCatalog LocalizationCatalog { get; }

	public TreeViewerBackend(
		IFileSystemScanner scanner,
		ITreeBuilder builder,
		ISelectedContentExportService contentExporter,
		IIconPackService icons,
		ILocalizationCatalog localizationCatalog)
	{
		Scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
		Builder = builder ?? throw new ArgumentNullException(nameof(builder));
		ContentExporter = contentExporter ?? throw new ArgumentNullException(nameof(contentExporter));

		Icons = icons ?? throw new ArgumentNullException(nameof(icons));
		LocalizationCatalog = localizationCatalog ?? throw new ArgumentNullException(nameof(localizationCatalog));

		RootScan = new RootScanService(Scanner);
	}

	public static TreeViewerBackend CreateDefault()
	{
		IFileSystemScanner scanner = new FileSystemScanner();
		ITreeBuilder builder = new TreeBuilder();
		ISelectedContentExportService exporter = new SelectedContentExportService();

		var icons = IconPackProvider.CreateDefault();

		// Catalog для локализации тот же, что использует LocalizationService по умолчанию
		var catalog = CreateDefaultLocalizationCatalog();

		return new TreeViewerBackend(scanner, builder, exporter, icons, catalog);
	}

	private static ILocalizationCatalog CreateDefaultLocalizationCatalog()
	{
		var store = AssetsResourceStoreFactory.CreateEmbeddedAssetsStore();
		return new ProjectTreeViewer.Infrastructure.Localization.ResourceLocalizationCatalog(store);
	}

	public RootScanData ScanRoot(string rootPath, bool ignoreBin, bool ignoreObj, bool ignoreDot)
		=> RootScan.Scan(rootPath, ignoreBin, ignoreObj, ignoreDot);

	public TreeBuildResult BuildTree(string rootPath, TreeFilterOptions options)
		=> Builder.Build(rootPath, options);

	public string BuildSelectedContent(IEnumerable<string> filePaths)
		=> ContentExporter.Build(filePaths);
}
