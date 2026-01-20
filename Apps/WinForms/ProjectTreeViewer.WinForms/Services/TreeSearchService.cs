using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ProjectTreeViewer.WinForms.Services;

public sealed class TreeSearchService
{
	// Flattened list of nodes for fast "contains text" search in the TreeView.
	private readonly List<TreeNode> _nodes = new();

	public void Rebuild(TreeView tree)
	{
		// Called after tree rebuilds to keep search in sync with visible nodes.
		_nodes.Clear();
		foreach (TreeNode node in tree.Nodes)
			Collect(node);
	}

	public IReadOnlyList<TreeNode> FindMatches(string query)
	{
		// Simple case-insensitive substring match over each node's display text.
		if (string.IsNullOrWhiteSpace(query))
			return Array.Empty<TreeNode>();

		return _nodes
			.Where(node => node.Text.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
			.ToList();
	}

	private void Collect(TreeNode node)
	{
		// Recursive depth-first collection of tree nodes.
		_nodes.Add(node);
		foreach (TreeNode child in node.Nodes)
			Collect(child);
	}
}
