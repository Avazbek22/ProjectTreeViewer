using System.IO;
using ProjectTreeViewer.Kernel.Abstractions;
using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Infrastructure.FileSystem;

public sealed class TreeBuilder : ITreeBuilder
{
	public TreeBuildResult Build(string rootPath, TreeFilterOptions options)
	{
		var state = new BuildState();

		var rootInfo = new DirectoryInfo(rootPath);
		var root = new FileSystemNode(
			name: rootInfo.Name,
			fullPath: rootPath,
			isDirectory: true,
			isAccessDenied: false,
			children: new List<FileSystemNode>());

		BuildChildren(
			parent: root,
			path: rootPath,
			options: options,
			isRoot: true,
			state: state);

		return new TreeBuildResult(root, state.RootAccessDenied, state.HadAccessDenied);
	}

	private static void BuildChildren(
		FileSystemNode parent,
		string path,
		TreeFilterOptions options,
		bool isRoot,
		BuildState state)
	{
		FileSystemInfo[] entries;
		try
		{
			entries = new DirectoryInfo(path)
				.GetFileSystemInfos()
				.Where(fi => (fi.Attributes & FileAttributes.Hidden) == 0)
				.OrderBy(fi => !fi.Attributes.HasFlag(FileAttributes.Directory))
				.ThenBy(fi => fi.Name, StringComparer.OrdinalIgnoreCase)
				.ToArray();
		}
		catch (UnauthorizedAccessException)
		{
			state.HadAccessDenied = true;
			if (isRoot) state.RootAccessDenied = true;
			parent.IsAccessDenied = true;
			return;
		}
		catch
		{
			return;
		}

		var children = (List<FileSystemNode>)parent.Children;

		foreach (var entry in entries)
		{
			var name = entry.Name;
			bool isDir = entry.Attributes.HasFlag(FileAttributes.Directory);

			if (isDir && isRoot && !options.AllowedRootFolders.Contains(name))
				continue;

			if (isDir)
			{
				if ((options.IgnoreBin && name.Equals("bin", StringComparison.OrdinalIgnoreCase)) ||
					(options.IgnoreObj && name.Equals("obj", StringComparison.OrdinalIgnoreCase)) ||
					(options.IgnoreDot && name.StartsWith(".", StringComparison.Ordinal)))
					continue;

				var dirNode = new FileSystemNode(
					name: name,
					fullPath: entry.FullName,
					isDirectory: true,
					isAccessDenied: false,
					children: new List<FileSystemNode>());

				children.Add(dirNode);

				BuildChildren(dirNode, entry.FullName, options, isRoot: false, state);
			}
			else
			{
				if (options.IgnoreDot && name.StartsWith(".", StringComparison.Ordinal))
					continue;

				if (options.AllowedExtensions.Count == 0)
					continue;

				var ext = Path.GetExtension(name);
				if (!options.AllowedExtensions.Contains(ext))
					continue;

				children.Add(new FileSystemNode(
					name: name,
					fullPath: entry.FullName,
					isDirectory: false,
					isAccessDenied: false,
					children: new List<FileSystemNode>()));
			}
		}
	}

	private sealed class BuildState
	{
		public bool RootAccessDenied { get; set; }
		public bool HadAccessDenied { get; set; }
	}
}
