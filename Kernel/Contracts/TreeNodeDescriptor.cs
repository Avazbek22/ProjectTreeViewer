namespace DevProjex.Kernel.Contracts;

public sealed record TreeNodeDescriptor(
	string DisplayName,
	string FullPath,
	bool IsDirectory,
	bool IsAccessDenied,
	string IconKey,
	IReadOnlyList<TreeNodeDescriptor> Children);
