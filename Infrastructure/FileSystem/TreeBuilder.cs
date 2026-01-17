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

			var ignore = options.IgnoreRules;

			if (isDir)
			{
				if (ShouldSkipDirectory(entry, ignore))
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
				if (ShouldSkipFile(entry, ignore))
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

	private static bool ShouldSkipDirectory(FileSystemInfo entry, IgnoreRules rules)
	{
		if (rules.SmartIgnoredFolders.Contains(entry.Name))
			return true;

		if (rules.IgnoreBinFolders && entry.Name.Equals("bin", StringComparison.OrdinalIgnoreCase))
			return true;

		if (rules.IgnoreObjFolders && entry.Name.Equals("obj", StringComparison.OrdinalIgnoreCase))
			return true;

		if (rules.IgnoreDotFolders && entry.Name.StartsWith(".", StringComparison.Ordinal))
			return true;

		if (rules.IgnoreHiddenFolders && entry.Attributes.HasFlag(FileAttributes.Hidden))
			return true;

		return false;
	}

	private static bool ShouldSkipFile(FileSystemInfo entry, IgnoreRules rules)
	{
		if (rules.SmartIgnoredFiles.Contains(entry.Name))
			return true;

		if (rules.IgnoreDotFiles && entry.Name.StartsWith(".", StringComparison.Ordinal))
			return true;

		if (rules.IgnoreHiddenFiles && entry.Attributes.HasFlag(FileAttributes.Hidden))
			return true;

		return false;
	}

	private sealed class BuildState
	{
		public bool RootAccessDenied { get; set; }
		public bool HadAccessDenied { get; set; }
	}
}
