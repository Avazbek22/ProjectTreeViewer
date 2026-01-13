using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ProjectTreeViewer;

public sealed class TreeExportService
{
	public string BuildFullTree(string rootPath, TreeNode root)
	{
		var sb = new StringBuilder();
		sb.AppendLine($"{rootPath}:");
		sb.AppendLine();

		sb.AppendLine($"├── {root.Text}");
		AppendAscii(root, "│   ", sb);

		return sb.ToString();
	}

	public string BuildSelectedTree(string rootPath, TreeNode root)
	{
		if (!HasCheckedDescendantOrSelf(root))
			return string.Empty;

		var sb = new StringBuilder();
		sb.AppendLine($"{rootPath}:");
		sb.AppendLine();

		sb.AppendLine($"├── {root.Text}");
		AppendSelectedAscii(root, "│   ", sb);

		return sb.ToString();
	}

	private static void AppendAscii(TreeNode node, string indent, StringBuilder sb)
	{
		for (int i = 0; i < node.Nodes.Count; i++)
		{
			var child = node.Nodes[i];
			bool last = i == node.Nodes.Count - 1;

			sb.Append(indent).Append(last ? "└── " : "├── ").AppendLine(child.Text);

			if (child.Nodes.Count > 0)
			{
				var nextIndent = indent + (last ? "    " : "│   ");
				AppendAscii(child, nextIndent, sb);
			}
		}
	}

	private static void AppendSelectedAscii(TreeNode node, string indent, StringBuilder sb)
	{
		var visible = node.Nodes
			.Cast<TreeNode>()
			.Where(HasCheckedDescendantOrSelf)
			.ToList();

		for (int i = 0; i < visible.Count; i++)
		{
			var child = visible[i];
			bool last = i == visible.Count - 1;

			sb.Append(indent).Append(last ? "└── " : "├── ").AppendLine(child.Text);

			if (child.Nodes.Count > 0)
			{
				var nextIndent = indent + (last ? "    " : "│   ");
				AppendSelectedAscii(child, nextIndent, sb);
			}
		}
	}

	public static bool HasCheckedDescendantOrSelf(TreeNode node)
	{
		if (node.Checked) return true;

		foreach (TreeNode child in node.Nodes)
		{
			if (HasCheckedDescendantOrSelf(child))
				return true;
		}

		return false;
	}

	public static IEnumerable<string> GetCheckedFilePaths(TreeNodeCollection nodes)
	{
		foreach (TreeNode node in nodes)
		{
			if (node.Checked && node.Tag is string path && System.IO.File.Exists(path))
				yield return path;

			foreach (var child in GetCheckedFilePaths(node.Nodes))
				yield return child;
		}
	}
}
