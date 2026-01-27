using System.Text;
using DevProjex.Kernel.Contracts;

namespace DevProjex.Application.Services;

public sealed class TreeExportService
{
	public string BuildFullTree(string rootPath, TreeNodeDescriptor root)
	{
		var sb = new StringBuilder();
		sb.Append(rootPath).AppendLine(":");
		sb.AppendLine();

		sb.Append("├── ").AppendLine(root.DisplayName);
		AppendAscii(root, "│   ", sb);

		return sb.ToString();
	}

	public string BuildSelectedTree(string rootPath, TreeNodeDescriptor root, IReadOnlySet<string> selectedPaths)
	{
		if (!HasSelectedDescendantOrSelf(root, selectedPaths))
			return string.Empty;

		var sb = new StringBuilder();
		sb.Append(rootPath).AppendLine(":");
		sb.AppendLine();

		sb.Append("├── ").AppendLine(root.DisplayName);
		AppendSelectedAscii(root, selectedPaths, "│   ", sb);

		return sb.ToString();
	}

	public static bool HasSelectedDescendantOrSelf(TreeNodeDescriptor node, IReadOnlySet<string> selectedPaths)
	{
		if (selectedPaths.Contains(node.FullPath)) return true;

		foreach (var child in node.Children)
		{
			if (HasSelectedDescendantOrSelf(child, selectedPaths))
				return true;
		}

		return false;
	}

	private static void AppendAscii(TreeNodeDescriptor node, string indent, StringBuilder sb)
	{
		for (int i = 0; i < node.Children.Count; i++)
		{
			var child = node.Children[i];
			bool last = i == node.Children.Count - 1;

			sb.Append(indent).Append(last ? "└── " : "├── ").AppendLine(child.DisplayName);

			if (child.Children.Count > 0)
			{
				var nextIndent = string.Concat(indent, last ? "    " : "│   ");
				AppendAscii(child, nextIndent, sb);
			}
		}
	}

	private static void AppendSelectedAscii(TreeNodeDescriptor node, IReadOnlySet<string> selectedPaths, string indent, StringBuilder sb)
	{
		// Count visible children without allocating a list
		int visibleCount = 0;
		foreach (var child in node.Children)
		{
			if (HasSelectedDescendantOrSelf(child, selectedPaths))
				visibleCount++;
		}

		int currentIndex = 0;
		foreach (var child in node.Children)
		{
			if (!HasSelectedDescendantOrSelf(child, selectedPaths))
				continue;

			currentIndex++;
			bool last = currentIndex == visibleCount;

			sb.Append(indent).Append(last ? "└── " : "├── ").AppendLine(child.DisplayName);

			if (child.Children.Count > 0)
			{
				var nextIndent = string.Concat(indent, last ? "    " : "│   ");
				AppendSelectedAscii(child, selectedPaths, nextIndent, sb);
			}
		}
	}
}
