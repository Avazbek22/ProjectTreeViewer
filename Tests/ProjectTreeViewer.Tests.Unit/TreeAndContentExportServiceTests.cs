using System.Collections.Generic;
using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Kernel.Contracts;
using ProjectTreeViewer.Tests.Unit.Helpers;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit;

public sealed class TreeAndContentExportServiceTests
{
	[Fact]
	public void Build_UsesSelectedTreeAndContentWhenSelectionsProvided()
	{
		using var temp = new TemporaryDirectory();
		var file = temp.CreateFile("file.txt", "Hello");

		var root = new TreeNodeDescriptor(
			DisplayName: "root",
			FullPath: temp.Path,
			IsDirectory: true,
			IsAccessDenied: false,
			IconKey: "folder",
			Children: new List<TreeNodeDescriptor>
			{
				new TreeNodeDescriptor("file.txt", file, false, false, "text", new List<TreeNodeDescriptor>())
			});

		var service = new TreeAndContentExportService(new TreeExportService(), new SelectedContentExportService());

		var result = service.Build(temp.Path, root, new HashSet<string> { file });

		Assert.Contains("file.txt", result);
		Assert.Contains("Hello", result);
	}

	[Fact]
	public void Build_FallsBackToFullTreeWhenSelectionEmpty()
	{
		var root = new TreeNodeDescriptor(
			DisplayName: "root",
			FullPath: "/root",
			IsDirectory: true,
			IsAccessDenied: false,
			IconKey: "folder",
			Children: new List<TreeNodeDescriptor>());

		var service = new TreeAndContentExportService(new TreeExportService(), new SelectedContentExportService());
		var result = service.Build("/root", root, new HashSet<string>());

		Assert.Contains("/root:", result);
	}
}
