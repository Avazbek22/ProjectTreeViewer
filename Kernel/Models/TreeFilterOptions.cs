namespace ProjectTreeViewer.Kernel.Models;

public sealed record TreeFilterOptions(
	IReadOnlySet<string> AllowedExtensions,
	IReadOnlySet<string> AllowedRootFolders,
	bool IgnoreBin,
	bool IgnoreObj,
	bool IgnoreDot);
