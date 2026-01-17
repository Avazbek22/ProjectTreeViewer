namespace ProjectTreeViewer.Kernel.Contracts;

public sealed record BuildTreeResult(
	TreeNodeDescriptor Root,
	bool RootAccessDenied,
	bool HadAccessDenied);
