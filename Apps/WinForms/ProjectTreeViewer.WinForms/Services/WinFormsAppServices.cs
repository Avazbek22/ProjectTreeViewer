using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Application.UseCases;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer.WinForms.Services;

public sealed record WinFormsAppServices(
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
	TreeViewRenderer TreeViewRenderer,
	TreeSelectionService TreeSelectionService,
	IIconStore IconStore);
