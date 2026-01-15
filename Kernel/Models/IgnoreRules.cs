namespace ProjectTreeViewer.Kernel.Models;

public sealed record IgnoreRules(
	bool IgnoreBin,
	bool IgnoreObj,
	bool IgnoreDot);
