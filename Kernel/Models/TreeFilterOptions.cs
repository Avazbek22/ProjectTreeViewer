namespace ProjectTreeViewer.Kernel.Models;

public sealed record TreeFilterOptions(
	IReadOnlySet<string> AllowedExtensions,
	IReadOnlySet<string> AllowedRootFolders,
	IgnoreRules IgnoreRules,
	string? NameFilter = null);
