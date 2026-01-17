namespace ProjectTreeViewer.Kernel.Models;

public sealed record IgnoreOptionDefinition(
	string Id,
	IgnoreOptionKind Kind,
	bool DefaultChecked);
