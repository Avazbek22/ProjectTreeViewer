namespace DevProjex.Kernel.Models;

public sealed record IgnoreRules(
	bool IgnoreBinFolders,
	bool IgnoreObjFolders,
	bool IgnoreHiddenFolders,
	bool IgnoreHiddenFiles,
	bool IgnoreDotFolders,
	bool IgnoreDotFiles,
	IReadOnlySet<string> SmartIgnoredFolders,
	IReadOnlySet<string> SmartIgnoredFiles);
