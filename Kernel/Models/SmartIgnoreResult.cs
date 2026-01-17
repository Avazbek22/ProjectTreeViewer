namespace ProjectTreeViewer.Kernel.Models;

public sealed record SmartIgnoreResult(
	IReadOnlySet<string> FolderNames,
	IReadOnlySet<string> FileNames);
