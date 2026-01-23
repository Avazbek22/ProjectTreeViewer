using System.Collections.Generic;
using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Kernel.Contracts;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit;

public sealed class TreeExportServiceTests
{
	// Verifies the full tree export renders ASCII output with the root and children.
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

	// Verifies selected tree export only includes selected paths.
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

	// Verifies selected tree export returns empty when nothing is selected.
	[Fact]
	public void BuildSelectedTree_ReturnsEmptyWhenNoSelections()
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

		var result = service.BuildSelectedTree("/root", root, new HashSet<string>());

		Assert.Equal(string.Empty, result);
	}

	// Verifies selection matching returns true for a descendant.
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

	// Verifies selection matching returns false when nothing is selected.
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

	// Verifies the full tree export renders nested folder structure.
	[Fact]
	public void BuildFullTree_RendersNestedDirectories()
	{
		var root = new TreeNodeDescriptor(
			DisplayName: "root",
			FullPath: "/root",
			IsDirectory: true,
			IsAccessDenied: false,
			IconKey: "folder",
			Children: new List<TreeNodeDescriptor>
			{
				new TreeNodeDescriptor(
					"src",
					"/root/src",
					true,
					false,
					"folder",
					new List<TreeNodeDescriptor>
					{
						new TreeNodeDescriptor("main.cs", "/root/src/main.cs", false, false, "csharp", new List<TreeNodeDescriptor>())
					})
			});

		var service = new TreeExportService();
		var result = service.BuildFullTree("/root", root);

		Assert.Contains("└── src", result);
		Assert.Contains("│       └── main.cs", result);
	}

	// Verifies selected tree includes ancestor directories for selected descendants.
	[Fact]
	public void BuildSelectedTree_IncludesAncestorDirectories()
	{
		var root = new TreeNodeDescriptor(
			DisplayName: "root",
			FullPath: "/root",
			IsDirectory: true,
			IsAccessDenied: false,
			IconKey: "folder",
			Children: new List<TreeNodeDescriptor>
			{
				new TreeNodeDescriptor(
					"src",
					"/root/src",
					true,
					false,
					"folder",
					new List<TreeNodeDescriptor>
					{
						new TreeNodeDescriptor("main.cs", "/root/src/main.cs", false, false, "csharp", new List<TreeNodeDescriptor>())
					})
			});

		var service = new TreeExportService();
		var selected = new HashSet<string> { "/root/src/main.cs" };
		var result = service.BuildSelectedTree("/root", root, selected);

		Assert.Contains("src", result);
		Assert.Contains("main.cs", result);
	}

	// Verifies selecting the root includes the root node only.
	[Fact]
	public void BuildSelectedTree_ReturnsRootOnlyWhenRootSelected()
	{
		var root = new TreeNodeDescriptor(
			DisplayName: "root",
			FullPath: "/root",
			IsDirectory: true,
			IsAccessDenied: false,
			IconKey: "folder",
			Children: new List<TreeNodeDescriptor>
			{
				new TreeNodeDescriptor("child", "/root/child", false, false, "text", new List<TreeNodeDescriptor>())
			});

		var service = new TreeExportService();
		var selected = new HashSet<string> { "/root" };
		var result = service.BuildSelectedTree("/root", root, selected);

		Assert.Contains("root", result);
		Assert.DoesNotContain("child", result);
	}

	// Verifies selection matching returns true when the node itself is selected.
	[Fact]
	public void HasSelectedDescendantOrSelf_ReturnsTrueWhenSelfSelected()
	{
		var node = new TreeNodeDescriptor(
			DisplayName: "root",
			FullPath: "/root",
			IsDirectory: true,
			IsAccessDenied: false,
			IconKey: "folder",
			Children: new List<TreeNodeDescriptor>());

		var selected = new HashSet<string> { "/root" };

		Assert.True(TreeExportService.HasSelectedDescendantOrSelf(node, selected));
	}
}
