using System.Collections.Generic;
using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Kernel.Contracts;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit;

public sealed class TreeExportServiceTests
{
	[Fact]
	public void BuildFullTree_ReturnsAsciiTree()
	{
		var root = new TreeNodeDescriptor(
			DisplayName: "root",
			FullPath: "/root",
			IsDirectory: true,
			IsAccessDenied: false,
			IconKey: "folder",
			Children: new List<TreeNodeDescriptor>
			{
				new TreeNodeDescriptor("file.txt", "/root/file.txt", false, false, "text", new List<TreeNodeDescriptor>())
			});

		var service = new TreeExportService();
		var result = service.BuildFullTree("/root", root);

		Assert.Contains("/root:", result);
		Assert.Contains("└── file.txt", result);
	}

	[Fact]
	public void BuildSelectedTree_ReturnsOnlySelectedPaths()
	{
		var root = new TreeNodeDescriptor(
			DisplayName: "root",
			FullPath: "/root",
			IsDirectory: true,
			IsAccessDenied: false,
			IconKey: "folder",
			Children: new List<TreeNodeDescriptor>
			{
				new TreeNodeDescriptor("keep.txt", "/root/keep.txt", false, false, "text", new List<TreeNodeDescriptor>()),
				new TreeNodeDescriptor("skip.txt", "/root/skip.txt", false, false, "text", new List<TreeNodeDescriptor>())
			});

		var service = new TreeExportService();
		var selected = new HashSet<string> { "/root/keep.txt" };
		var result = service.BuildSelectedTree("/root", root, selected);

		Assert.Contains("keep.txt", result);
		Assert.DoesNotContain("skip.txt", result);
	}

	[Fact]
	public void HasSelectedDescendantOrSelf_ReturnsTrueWhenMatch()
	{
		var node = new TreeNodeDescriptor(
			DisplayName: "root",
			FullPath: "/root",
			IsDirectory: true,
			IsAccessDenied: false,
			IconKey: "folder",
			Children: new List<TreeNodeDescriptor>
			{
				new TreeNodeDescriptor("child", "/root/child", false, false, "text", new List<TreeNodeDescriptor>())
			});

		var selected = new HashSet<string> { "/root/child" };

		Assert.True(TreeExportService.HasSelectedDescendantOrSelf(node, selected));
	}

	[Fact]
	public void HasSelectedDescendantOrSelf_ReturnsFalseWhenNoMatch()
	{
		var node = new TreeNodeDescriptor(
			DisplayName: "root",
			FullPath: "/root",
			IsDirectory: true,
			IsAccessDenied: false,
			IconKey: "folder",
			Children: new List<TreeNodeDescriptor>());

		var selected = new HashSet<string>();

		Assert.False(TreeExportService.HasSelectedDescendantOrSelf(node, selected));
	}
}
