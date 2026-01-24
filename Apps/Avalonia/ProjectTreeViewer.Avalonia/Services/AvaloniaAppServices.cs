using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Application.UseCases;
using ProjectTreeViewer.Infrastructure.ResourceStore;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer.Avalonia.Services;

public sealed record AvaloniaAppServices(
    LocalizationService Localization,
    HelpContentProvider HelpContentProvider,
    IElevationService Elevation,
    ScanOptionsUseCase ScanOptionsUseCase,
    BuildTreeUseCase BuildTreeUseCase,
    IgnoreOptionsService IgnoreOptionsService,
    IgnoreRulesService IgnoreRulesService,
    FilterOptionSelectionService FilterOptionSelectionService,
    TreeExportService TreeExportService,
    SelectedContentExportService ContentExportService,
    TreeAndContentExportService TreeAndContentExportService,
    IIconStore IconStore);
