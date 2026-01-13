namespace ProjectTreeViewer;

public sealed record TreeBuildResult(
	FileSystemNode Root,
	bool RootAccessDenied,
	bool HadAccessDenied);

public sealed record FileSystemNode(
	string Name,
	string FullPath,
	bool IsDirectory,
	bool IsAccessDenied,
	System.Collections.Generic.List<FileSystemNode> Children);
