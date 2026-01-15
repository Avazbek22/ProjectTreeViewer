using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Application.UseCases;
using ProjectTreeViewer.Infrastructure.Elevation;
using ProjectTreeViewer.Infrastructure.FileSystem;
using ProjectTreeViewer.Infrastructure.ResourceStore;
using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.WinForms.Services;

public static class WinFormsCompositionRoot
{
	public static WinFormsAppServices CreateDefault(CommandLineOptions options)
	{
		var localizationCatalog = new JsonLocalizationCatalog();
		var localization = new LocalizationService(localizationCatalog, options.Language ?? CommandLineOptions.DetectSystemLanguage());
		var iconStore = new EmbeddedIconStore();
		var iconMapper = new IconMapper();
		var treePresenter = new TreeNodePresentationService(localization, iconMapper);
		var scanner = new FileSystemScanner();
		var treeBuilder = new TreeBuilder();
		var scanOptionsUseCase = new ScanOptionsUseCase(scanner);
		var buildTreeUseCase = new BuildTreeUseCase(treeBuilder, treePresenter);
		var treeExportService = new TreeExportService();
		var contentExportService = new SelectedContentExportService(localization);
		var treeAndContentExportService = new TreeAndContentExportService(treeExportService, contentExportService);
		var renderer = new TreeViewRenderer();
		var selection = new TreeSelectionService();
		var elevation = new ElevationService();

		return new WinFormsAppServices(
			Localization: localization,
			Elevation: elevation,
			ScanOptionsUseCase: scanOptionsUseCase,
			BuildTreeUseCase: buildTreeUseCase,
			TreeExportService: treeExportService,
			ContentExportService: contentExportService,
			TreeAndContentExportService: treeAndContentExportService,
			TreeViewRenderer: renderer,
			TreeSelectionService: selection,
			IconStore: iconStore);
	}
}
