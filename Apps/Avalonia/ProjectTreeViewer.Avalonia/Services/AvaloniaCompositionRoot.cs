using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Application.UseCases;
using ProjectTreeViewer.Infrastructure.Elevation;
using ProjectTreeViewer.Infrastructure.FileSystem;
using ProjectTreeViewer.Infrastructure.ResourceStore;
using ProjectTreeViewer.Infrastructure.SmartIgnore;
using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Avalonia.Services;

public static class AvaloniaCompositionRoot
{
    public static AvaloniaAppServices CreateDefault(CommandLineOptions options)
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
        var elevation = new ElevationService();

        return new AvaloniaAppServices(
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
            IconStore: iconStore);
    }
}
