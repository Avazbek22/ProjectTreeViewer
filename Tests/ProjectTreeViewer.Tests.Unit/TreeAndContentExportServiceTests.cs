using System;
using System.Collections.Generic;
using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Kernel.Contracts;
using ProjectTreeViewer.Tests.Unit.Helpers;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit;

public sealed class TreeAndContentExportServiceTests
{
	// Verifies selected files drive both tree and content exports.
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

	// Verifies full tree is used when no selections are provided.
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

	// Verifies tree output is returned when selected content is empty.
	[Fact]
	public void Build_ReturnsTreeWhenSelectedContentEmpty()
	{
		var root = new TreeNodeDescriptor(
			DisplayName: "root",
			FullPath: "/root",
			IsDirectory: true,
			IsAccessDenied: false,
			IconKey: "folder",
			Children: new List<TreeNodeDescriptor>());

		var service = new TreeAndContentExportService(new TreeExportService(), new SelectedContentExportService());
		var result = service.Build("/root", root, new HashSet<string> { "/root/missing.txt" });

		Assert.Contains("/root:", result);
		Assert.DoesNotContain("missing.txt:", result);
	}

	// Verifies a selection not in the tree falls back to full tree and all-file content.
	[Fact]
	public void Build_UsesFullTreeWhenSelectionsNotInTree()
	{
		using var temp = new TemporaryDirectory();
		var file = temp.CreateFile("notes.txt", "Note");

		var root = new TreeNodeDescriptor(
			DisplayName: "root",
			FullPath: temp.Path,
			IsDirectory: true,
			IsAccessDenied: false,
			IconKey: "folder",
			Children: new List<TreeNodeDescriptor>
			{
				new TreeNodeDescriptor("notes.txt", file, false, false, "text", new List<TreeNodeDescriptor>())
			});

		var service = new TreeAndContentExportService(new TreeExportService(), new SelectedContentExportService());
		var result = service.Build(temp.Path, root, new HashSet<string> { "/missing/file.txt" });

		Assert.Contains("notes.txt", result);
		Assert.Contains("Note", result);
	}

	// Verifies full-tree exports include content for all files when no selections exist.
	[Fact]
	public void Build_UsesAllFilesWhenNoSelection()
	{
		using var temp = new TemporaryDirectory();
		var first = temp.CreateFile("a.txt", "A");
		var second = temp.CreateFile("b.txt", "B");

		var root = new TreeNodeDescriptor(
			DisplayName: "root",
			FullPath: temp.Path,
			IsDirectory: true,
			IsAccessDenied: false,
			IconKey: "folder",
			Children: new List<TreeNodeDescriptor>
			{
				new TreeNodeDescriptor("a.txt", first, false, false, "text", new List<TreeNodeDescriptor>()),
				new TreeNodeDescriptor("b.txt", second, false, false, "text", new List<TreeNodeDescriptor>())
			});

		var service = new TreeAndContentExportService(new TreeExportService(), new SelectedContentExportService());
		var result = service.Build(temp.Path, root, new HashSet<string>());

		Assert.Contains("a.txt:", result);
		Assert.Contains("b.txt:", result);
		Assert.Contains("A", result);
		Assert.Contains("B", result);
	}

	// Verifies clipboard spacing separates tree and content sections.
	[Fact]
	public void Build_IncludesClipboardSpacingBetweenTreeAndContent()
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
		var result = service.Build(temp.Path, root, new HashSet<string>());


		var nl = Environment.NewLine;
		Assert.Contains($"\u00A0{nl}\u00A0{nl}", result);
	}
}
