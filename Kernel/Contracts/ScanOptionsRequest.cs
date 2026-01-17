using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Kernel.Contracts;

public sealed record ScanOptionsRequest(
	string RootPath,
	IgnoreRules IgnoreRules,
	IReadOnlySet<string> AllowedRootFolders);
