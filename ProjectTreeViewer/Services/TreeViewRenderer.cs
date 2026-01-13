using System.Windows.Forms;

namespace ProjectTreeViewer;

public sealed class TreeViewRenderer
{
	public void Render(TreeView treeView, FileSystemNode rootNode, bool expandAll)
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

	private static TreeNode CreateTreeNode(FileSystemNode model)
	{
		var node = new TreeNode(model.Name) { Tag = model.FullPath };

		foreach (var child in model.Children)
			node.Nodes.Add(CreateTreeNode(child));

		return node;
	}
}
