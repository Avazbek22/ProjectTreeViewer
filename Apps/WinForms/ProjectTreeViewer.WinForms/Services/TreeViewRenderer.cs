using System.Windows.Forms;
using ProjectTreeViewer.Kernel.Contracts;

namespace ProjectTreeViewer.WinForms.Services;

public sealed class TreeViewRenderer
{
	public void Render(TreeView treeView, TreeNodeDescriptor rootNode, bool expandAll)
	{
		// Centralized UI rendering: rebuilds the entire TreeView from a descriptor model.
		treeView.BeginUpdate();
		try
		{
			treeView.Nodes.Clear();

			var root = CreateTreeNode(rootNode);
			treeView.Nodes.Add(root);

			if (expandAll)
				root.ExpandAll();
			else
				root.Expand();
		}
		finally
		{
			treeView.EndUpdate();
		}
	}

	private static TreeNode CreateTreeNode(TreeNodeDescriptor model)
	{
		// Each node carries full path in Tag and icon keys for normal/selected states.
		var node = new TreeNode(model.DisplayName)
		{
			Tag = model.FullPath,
			ImageKey = model.IconKey,
			SelectedImageKey = model.IconKey
		};

		// Recursively project child descriptors into child TreeNodes.
		foreach (var child in model.Children)
			node.Nodes.Add(CreateTreeNode(child));

		return node;
	}
}
