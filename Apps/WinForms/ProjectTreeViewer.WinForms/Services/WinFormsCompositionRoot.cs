using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Application.UseCases;
using ProjectTreeViewer.Infrastructure.Elevation;
using ProjectTreeViewer.Infrastructure.FileSystem;
using ProjectTreeViewer.Infrastructure.ResourceStore;
using ProjectTreeViewer.Infrastructure.SmartIgnore;
using ProjectTreeViewer.Kernel.Abstractions;
using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.WinForms.Services;

public static class WinFormsCompositionRoot
{
	public static WinFormsAppServices CreateDefault(CommandLineOptions options)
	{
		// Composition root wires application, infrastructure, and WinForms services together.
		// This keeps Form1 focused on UI behavior rather than object construction.
		var localizationCatalog = new JsonLocalizationCatalog();
		var localization = new LocalizationService(localizationCatalog, options.Language ?? CommandLineOptions.DetectSystemLanguage());
		var iconStore = new EmbeddedIconStore();
		var iconMapper = new IconMapper();
		var treePresenter = new TreeNodePresentationService(localization, iconMapper);
		var scanner = new FileSystemScanner();
		var treeBuilder = new TreeBuilder();
		var scanOptionsUseCase = new ScanOptionsUseCase(scanner);
		var buildTreeUseCase = new BuildTreeUseCase(treeBuilder, treePresenter);
		// Smart ignore rules define common folders/artifacts the UI can toggle via ignore options.
		var smartIgnoreRules = new ISmartIgnoreRule[]
		{
			new CommonSmartIgnoreRule(),
			new FrontendArtifactsIgnoreRule()
		};
		var smartIgnoreService = new SmartIgnoreService(smartIgnoreRules);
		var ignoreOptionsService = new IgnoreOptionsService(localization);
		var ignoreRulesService = new IgnoreRulesService(smartIgnoreService);
		var filterSelectionService = new FilterOptionSelectionService();
		var treeExportService = new TreeExportService();
		var contentExportService = new SelectedContentExportService();
		var treeAndContentExportService = new TreeAndContentExportService(treeExportService, contentExportService);
		var renderer = new TreeViewRenderer();
		var selection = new TreeSelectionService();
		var elevation = new ElevationService();

		// Bundle all dependencies for Form1 so UI logic can focus on behavior.
		return new WinFormsAppServices(
			Localization: localization,
			Elevation: elevation,
			ScanOptionsUseCase: scanOptionsUseCase,
			BuildTreeUseCase: buildTreeUseCase,
			IgnoreOptionsService: ignoreOptionsService,
			IgnoreRulesService: ignoreRulesService,
			FilterOptionSelectionService: filterSelectionService,
			TreeExportService: treeExportService,
			ContentExportService: contentExportService,
			TreeAndContentExportService: treeAndContentExportService,
			TreeViewRenderer: renderer,
			TreeSelectionService: selection,
			IconStore: iconStore);
	}
}
