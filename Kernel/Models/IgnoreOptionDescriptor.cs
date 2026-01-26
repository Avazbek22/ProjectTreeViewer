namespace DevProjex.Kernel.Models;

public sealed record IgnoreOptionDescriptor(
	IgnoreOptionId Id,
	string Label,
	bool DefaultChecked);
