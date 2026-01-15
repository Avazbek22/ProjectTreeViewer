using System.IO;
using System.Text;
using ProjectTreeViewer.Kernel.Contracts;

namespace ProjectTreeViewer.Application.Services;

public sealed class TreeAndContentExportService
{
	private const string ClipboardBlankLine = "\u00A0"; // NBSP: looks empty but won't collapse on paste

	private readonly TreeExportService _treeExport;
	private readonly SelectedContentExportService _contentExport;

	public TreeAndContentExportService(TreeExportService treeExport, SelectedContentExportService contentExport)
	{
		_treeExport = treeExport;
		_contentExport = contentExport;
	}

	public string Build(string rootPath, TreeNodeDescriptor root, IReadOnlySet<string> selectedPaths)
	{
		bool hasSelection = selectedPaths.Count > 0 && TreeExportService.HasSelectedDescendantOrSelf(root, selectedPaths);

		string tree = hasSelection
			? _treeExport.BuildSelectedTree(rootPath, root, selectedPaths)
			: _treeExport.BuildFullTree(rootPath, root);

		if (hasSelection && string.IsNullOrWhiteSpace(tree))
			tree = _treeExport.BuildFullTree(rootPath, root);

		var files = hasSelection
			? GetSelectedFiles(selectedPaths)
			: GetAllFilePaths(root);

		var content = _contentExport.Build(files);
		if (string.IsNullOrWhiteSpace(content))
			return tree;

		var sb = new StringBuilder();
		sb.Append(tree.TrimEnd('\r', '\n'));
		AppendClipboardBlankLine(sb);
		AppendClipboardBlankLine(sb);
		sb.Append(content);

		return sb.ToString();
	}

	private static IEnumerable<string> GetSelectedFiles(IReadOnlySet<string> selectedPaths)
	{
		foreach (var path in selectedPaths)
		{
			if (File.Exists(path))
				yield return path;
		}
	}

	private static IEnumerable<string> GetAllFilePaths(TreeNodeDescriptor node)
	{
		if (!node.IsDirectory)
			yield return node.FullPath;

		foreach (var child in node.Children)
		{
			foreach (var path in GetAllFilePaths(child))
				yield return path;
		}
	}

	private static void AppendClipboardBlankLine(StringBuilder sb) => sb.AppendLine(ClipboardBlankLine);
}
