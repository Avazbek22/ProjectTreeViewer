using System.Windows.Forms;

namespace ProjectTreeViewer.WinForms.Services;

public sealed class TreeSelectionService
{
	public IEnumerable<string> GetCheckedPaths(TreeNodeCollection nodes)
	{
		foreach (TreeNode node in nodes)
		{
			if (node.Checked && node.Tag is string path)
				yield return path;

			foreach (var child in GetCheckedPaths(node.Nodes))
				yield return child;
		}
	}
}
