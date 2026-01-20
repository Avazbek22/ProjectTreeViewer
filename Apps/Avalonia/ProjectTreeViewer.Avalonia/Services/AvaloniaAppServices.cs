using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Application.UseCases;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer.Avalonia.Services;

public sealed record AvaloniaAppServices(
    LocalizationService Localization,
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
