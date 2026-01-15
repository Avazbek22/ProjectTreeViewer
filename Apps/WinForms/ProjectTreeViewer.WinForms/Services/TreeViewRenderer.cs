using System.Windows.Forms;
using ProjectTreeViewer.Kernel.Contracts;

namespace ProjectTreeViewer.WinForms.Services;

public sealed class TreeViewRenderer
{
	public void Render(TreeView treeView, TreeNodeDescriptor rootNode, bool expandAll)
	{
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
		var node = new TreeNode(model.DisplayName)
		{
			Tag = model.FullPath,
			ImageKey = model.IconKey,
			SelectedImageKey = model.IconKey
		};

		foreach (var child in model.Children)
			node.Nodes.Add(CreateTreeNode(child));

		return node;
	}
}
