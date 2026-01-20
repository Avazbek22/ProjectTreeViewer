using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Application.UseCases;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer.WinForms.Services;

// UI-focused service bundle passed into Form1 to keep composition in one place.
public sealed record WinFormsAppServices(
	// Localization and app-wide language switching.
	LocalizationService Localization,
	// Elevation support for protected folders.
	IElevationService Elevation,
	// Produces filter options (root folders, extensions) from a scan.
	ScanOptionsUseCase ScanOptionsUseCase,
	// Builds the tree model for rendering.
	BuildTreeUseCase BuildTreeUseCase,
	// UI-visible ignore options and rule assembly.
	IgnoreOptionsService IgnoreOptionsService,
	IgnoreRulesService IgnoreRulesService,
	// Keeps user selections stable when lists are refreshed.
	FilterOptionSelectionService FilterOptionSelectionService,
	// Export services for clipboard outputs.
	TreeExportService TreeExportService,
	SelectedContentExportService ContentExportService,
	TreeAndContentExportService TreeAndContentExportService,
	// WinForms-specific helpers for rendering and selection.
	TreeViewRenderer TreeViewRenderer,
	TreeSelectionService TreeSelectionService,
	// Icon store for file/folder glyphs in the TreeView.
	IIconStore IconStore);
