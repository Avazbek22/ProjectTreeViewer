using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer;

public sealed class TreeTextExportService : ITreeTextExportService
{
	public string BuildFullTree(string rootPath, FileSystemNode root)
	{
		var sb = new StringBuilder();

		sb.AppendLine($"{rootPath}:");
		sb.AppendLine();

		sb.AppendLine($"├── {root.Name}");
		AppendAscii(root, "│   ", sb);

		return sb.ToString();
	}

	public string BuildSelectedTree(string rootPath, FileSystemNode root, IEnumerable<string> selectedPaths)
	{
		var selected = NormalizeSelected(selectedPaths);

		if (selected.Count == 0)
			return string.Empty;

		// Если корень вообще не содержит выбранных элементов — пусто
		if (!IsNodeSelectedOrHasSelectedDescendant(root, selected))
			return string.Empty;

		var sb = new StringBuilder();

		sb.AppendLine($"{rootPath}:");
		sb.AppendLine();

		sb.AppendLine($"├── {root.Name}");
		AppendSelectedAscii(root, "│   ", sb, selected);

		return sb.ToString();
	}

	private static void AppendAscii(FileSystemNode node, string indent, StringBuilder sb)
	{
		for (int i = 0; i < node.Children.Count; i++)
		{
			var child = node.Children[i];
			bool last = i == node.Children.Count - 1;

			sb.Append(indent).Append(last ? "└── " : "├── ").AppendLine(child.Name);

			if (child.Children.Count > 0)
			{
				var nextIndent = indent + (last ? "    " : "│   ");
				AppendAscii(child, nextIndent, sb);
			}
		}
	}

	private static void AppendSelectedAscii(FileSystemNode node, string indent, StringBuilder sb, HashSet<string> selected)
	{
		var visible = node.Children
			.Where(c => IsNodeSelectedOrHasSelectedDescendant(c, selected))
			.ToList();

		for (int i = 0; i < visible.Count; i++)
		{
			var child = visible[i];
			bool last = i == visible.Count - 1;

			sb.Append(indent).Append(last ? "└── " : "├── ").AppendLine(child.Name);

			if (child.Children.Count > 0)
			{
				var nextIndent = indent + (last ? "    " : "│   ");
				AppendSelectedAscii(child, nextIndent, sb, selected);
			}
		}
	}

	private static bool IsNodeSelectedOrHasSelectedDescendant(FileSystemNode node, HashSet<string> selected)
	{
		// 1) Прямое попадание (файл/папка)
		if (IsSelected(node.FullPath, selected))
			return true;

		// 2) Если выбрана папка-родитель — тоже считается выбранным
		if (IsUnderSelectedDirectory(node.FullPath, selected))
			return true;

		// 3) Потомки
		foreach (var child in node.Children)
		{
			if (IsNodeSelectedOrHasSelectedDescendant(child, selected))
				return true;
		}

		return false;
	}

	private static bool IsSelected(string fullPath, HashSet<string> selected)
	{
		var p = NormalizePath(fullPath);
		return selected.Contains(p);
	}

	private static bool IsUnderSelectedDirectory(string fullPath, HashSet<string> selected)
	{
		var p = NormalizePath(fullPath);

		// Если кто-то выбрал директорию, то всё внутри тоже "выбрано"
		// (мы не знаем тут, файл это или папка — но правило работает и для файла, и для папки)
		foreach (var s in selected)
		{
			if (p.Length <= s.Length)
				continue;

			if (p.StartsWith(s, StringComparison.OrdinalIgnoreCase))
			{
				// граница директории
				if (p[s.Length] == Path.DirectorySeparatorChar)
					return true;
			}
		}

		return false;
	}

	private static HashSet<string> NormalizeSelected(IEnumerable<string> selectedPaths)
	{
		var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		if (selectedPaths is null)
			return set;

		foreach (var p in selectedPaths)
		{
			var np = NormalizePath(p);
			if (!string.IsNullOrWhiteSpace(np))
				set.Add(np);
		}

		return set;
	}

	private static string NormalizePath(string? path)
	{
		if (string.IsNullOrWhiteSpace(path))
			return string.Empty;

		try
		{
			return Path.GetFullPath(path.Trim());
		}
		catch
		{
			return path.Trim();
		}
	}
}
