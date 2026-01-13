using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProjectTreeViewer;

public sealed class TreeBuilder
{
	public TreeBuildResult Build(string rootPath, TreeFilterOptions options)
	{
		var state = new BuildState();

		var rootInfo = new DirectoryInfo(rootPath);
		var root = new FileSystemNode(
			Name: rootInfo.Name,
			FullPath: rootPath,
			IsDirectory: true,
			IsAccessDenied: false,
			Children: new List<FileSystemNode>());

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

			parent.Children.Add(new FileSystemNode(
				Name: "⛔ " + (isRoot ? "Нет доступа (корень)" : "Нет доступа"),
				FullPath: path,
				IsDirectory: true,
				IsAccessDenied: true,
				Children: new List<FileSystemNode>()));

			return;
		}
		catch
		{
			return;
		}

		foreach (var e in entries)
		{
			var name = e.Name;
			bool isDir = e.Attributes.HasFlag(FileAttributes.Directory);

			if (isDir && isRoot && !options.AllowedRootFolders.Contains(name))
				continue;

			if (isDir)
			{
				if ((options.IgnoreBin && name.Equals("bin", StringComparison.OrdinalIgnoreCase)) ||
					(options.IgnoreObj && name.Equals("obj", StringComparison.OrdinalIgnoreCase)) ||
					(options.IgnoreDot && name.StartsWith(".", StringComparison.Ordinal)))
					continue;

				var dirNode = new FileSystemNode(
					Name: name,
					FullPath: e.FullName,
					IsDirectory: true,
					IsAccessDenied: false,
					Children: new List<FileSystemNode>());

				parent.Children.Add(dirNode);

				BuildChildren(dirNode, e.FullName, options, isRoot: false, state);
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

				parent.Children.Add(new FileSystemNode(
					Name: name,
					FullPath: e.FullName,
					IsDirectory: false,
					IsAccessDenied: false,
					Children: new List<FileSystemNode>()));
			}
		}
	}

	private sealed class BuildState
	{
		public bool RootAccessDenied { get; set; }
		public bool HadAccessDenied { get; set; }
	}
}
