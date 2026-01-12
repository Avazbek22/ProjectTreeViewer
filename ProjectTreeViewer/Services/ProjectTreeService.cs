using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProjectTreeViewer;

public sealed class ProjectTreeService
{
	public ProjectTreeResult BuildTree(string rootPath, ProjectTreeOptions options)
	{
		var sb = new StringBuilder();

		if (options.IncludePathHeader)
			sb.AppendLine(rootPath);

		sb.AppendLine($"├── {new DirectoryInfo(rootPath).Name}");

		var state = new BuildState();

		PrintDirectory(
			rootPath,
			indent: "│   ",
			sb,
			options,
			isRoot: true,
			state);

		return new ProjectTreeResult(sb.ToString(), state.RootAccessDenied, state.HadAccessDenied);
	}

	private static void PrintDirectory(
		string path,
		string indent,
		StringBuilder sb,
		ProjectTreeOptions options,
		bool isRoot,
		BuildState state)
	{
		var entries = TryGetEntries(path, isRoot, state);
		if (entries.Count == 0) return;

		var ordered = entries
			.Where(e => !IsHiddenWindows(e))
			.OrderBy(e => !IsDirectory(e)) // папки первыми
			.ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
			.ToList();

		// Ключевое исправление: “последний” считаем по уже ОТОБРАННЫМ элементам
		var visible = ordered
			.Where(e => ShouldIncludeEntry(e, options, isRoot))
			.ToList();

		for (int i = 0; i < visible.Count; i++)
		{
			var e = visible[i];
			bool last = i == visible.Count - 1;

			sb.Append(indent)
			  .Append(last ? "└── " : "├── ")
			  .AppendLine(e.Name);

			if (!IsDirectory(e)) continue;

			var nextIndent = indent + (last ? "    " : "│   ");
			PrintDirectory(e.FullName, nextIndent, sb, options, isRoot: false, state);
		}
	}

	private static List<FileSystemInfo> TryGetEntries(string path, bool isRoot, BuildState state)
	{
		try
		{
			return new DirectoryInfo(path).GetFileSystemInfos().ToList();
		}
		catch (UnauthorizedAccessException)
		{
			state.HadAccessDenied = true;
			if (isRoot) state.RootAccessDenied = true;
			return [];
		}
		catch
		{
			return [];
		}
	}

	private static bool ShouldIncludeEntry(FileSystemInfo entry, ProjectTreeOptions options, bool isRoot)
	{
		bool isDir = IsDirectory(entry);
		string name = entry.Name;

		if (isDir)
		{
			// На корневом уровне показываем только отмеченные папки
			if (isRoot && !options.AllowedRootFolders.Contains(name))
				return false;

			if (options.IgnoreBin && name.Equals("bin", StringComparison.OrdinalIgnoreCase))
				return false;

			if (options.IgnoreObj && name.Equals("obj", StringComparison.OrdinalIgnoreCase))
				return false;

			if (options.IgnoreDot && name.StartsWith(".", StringComparison.Ordinal))
				return false;

			return true;
		}

		// Файлы
		if (options.IgnoreDot && name.StartsWith(".", StringComparison.Ordinal))
			return false;

		// Если ни один тип не выбран — вообще не показываем файлы.
		if (options.AllowedExtensions.Count == 0)
			return false;

		var ext = Path.GetExtension(name);
		return options.AllowedExtensions.Contains(ext);
	}

	private static bool IsDirectory(FileSystemInfo entry) => entry.Attributes.HasFlag(FileAttributes.Directory);

	private static bool IsHiddenWindows(FileSystemInfo entry) => entry.Attributes.HasFlag(FileAttributes.Hidden);

	private sealed class BuildState
	{
		public bool RootAccessDenied { get; set; }
		public bool HadAccessDenied { get; set; }
	}
}
