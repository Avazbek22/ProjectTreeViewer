namespace DevProjex.Kernel.Models;

public sealed record TreeBuildResult(
	FileSystemNode Root,
	bool RootAccessDenied,
	bool HadAccessDenied);

public sealed class FileSystemNode
{
	/// <summary>
	/// Shared empty list for file nodes (files have no children).
	/// Avoids allocating a new List for each of potentially 100k+ files.
	/// </summary>
	public static readonly IReadOnlyList<FileSystemNode> EmptyChildren = Array.Empty<FileSystemNode>();

	public FileSystemNode(
		string name,
		string fullPath,
		bool isDirectory,
		bool isAccessDenied,
		IReadOnlyList<FileSystemNode> children)
	{
		Name = name;
		FullPath = fullPath;
		IsDirectory = isDirectory;
		IsAccessDenied = isAccessDenied;
		Children = children;
	}

	public string Name { get; }
	public string FullPath { get; }
	public bool IsDirectory { get; }
	public bool IsAccessDenied { get; set; }
	public IReadOnlyList<FileSystemNode> Children { get; }
}
