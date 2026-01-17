using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ProjectTreeViewer.WinForms.Services;

public sealed class TreeSearchService
{
	private readonly List<TreeNode> _nodes = new();

	public void Rebuild(TreeView tree)
	{
		_nodes.Clear();
		foreach (TreeNode node in tree.Nodes)
			Collect(node);
	}

	public IReadOnlyList<TreeNode> FindMatches(string query)
	{
		if (string.IsNullOrWhiteSpace(query))
			return Array.Empty<TreeNode>();

		return _nodes
			.Where(node => node.Text.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
			.ToList();
	}

	private void Collect(TreeNode node)
	{
		_nodes.Add(node);
		foreach (TreeNode child in node.Nodes)
			Collect(child);
	}
}
