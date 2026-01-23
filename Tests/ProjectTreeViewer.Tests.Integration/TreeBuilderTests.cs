using System;
using System.Collections.Generic;
using System.Linq;
using ProjectTreeViewer.Infrastructure.FileSystem;
using ProjectTreeViewer.Kernel.Models;
using ProjectTreeViewer.Tests.Integration.Helpers;
using Xunit;

namespace ProjectTreeViewer.Tests.Integration;

public sealed class TreeBuilderTests
{
	// Verifies root-folder filtering applies to directories while root files still match extensions.
	[Fact]
	public void Build_FiltersByRootFoldersAndExtensions()
	{
		using var temp = new TemporaryDirectory();
		temp.CreateFile("src/app.cs", "class A {}");
		temp.CreateFile("src/readme.md", "hello");
		temp.CreateFile("docs/info.txt", "doc");
		temp.CreateFile("root.txt", "root");

		var options = new TreeFilterOptions(
			AllowedExtensions: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".txt" },
			AllowedRootFolders: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "docs" },
			IgnoreRules: new IgnoreRules(false, false, false, false, false, false, new HashSet<string>(), new HashSet<string>()));

		var builder = new TreeBuilder();
		var result = builder.Build(temp.Path, options);

		var children = result.Root.Children.Select(c => c.Name).ToList();
		Assert.Contains("docs", children);
		Assert.DoesNotContain("src", children);
		Assert.Contains("root.txt", children);

		var docs = result.Root.Children.First(c => c.Name == "docs");
		Assert.Single(docs.Children);
		Assert.Equal("info.txt", docs.Children[0].Name);
	}

	// Verifies directories are ordered before files in the root listing.
	[Fact]
	public void Build_OrdersDirectoriesBeforeFiles()
	{
		using var temp = new TemporaryDirectory();
		temp.CreateFile("a.txt", "a");
		temp.CreateFile("folder/b.txt", "b");

		var options = new TreeFilterOptions(
			AllowedExtensions: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".txt" },
			AllowedRootFolders: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "folder" },
			IgnoreRules: new IgnoreRules(false, false, false, false, false, false, new HashSet<string>(), new HashSet<string>()));

		var builder = new TreeBuilder();
		var result = builder.Build(temp.Path, options);

		Assert.Equal("folder", result.Root.Children.First().Name);
		Assert.False(result.Root.Children.Last().IsDirectory);
	}

	// Verifies no files are included when no extensions are allowed.
	[Fact]
	public void Build_SkipsFilesWhenAllowedExtensionsEmpty()
	{
		using var temp = new TemporaryDirectory();
		temp.CreateFile("root.txt", "root");
		temp.CreateFile("src/app.cs", "class A {}");

		var options = new TreeFilterOptions(
			AllowedExtensions: new HashSet<string>(),
			AllowedRootFolders: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "src" },
			IgnoreRules: new IgnoreRules(false, false, false, false, false, false, new HashSet<string>(), new HashSet<string>()));

		var builder = new TreeBuilder();
		var result = builder.Build(temp.Path, options);

		Assert.DoesNotContain(result.Root.Children, child => !child.IsDirectory);
	}

	// Verifies dot folders are excluded when the ignore rule is set.
	[Fact]
	public void Build_RespectsDotFolderIgnoreRule()
	{
		using var temp = new TemporaryDirectory();
		temp.CreateFile(".cache/hidden.txt", "hidden");
		temp.CreateFile("visible.txt", "visible");

		var options = new TreeFilterOptions(
			AllowedExtensions: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".txt" },
			AllowedRootFolders: new HashSet<string>(StringComparer.OrdinalIgnoreCase),
			IgnoreRules: new IgnoreRules(false, false, false, false, true, false, new HashSet<string>(), new HashSet<string>()));

		var builder = new TreeBuilder();
		var result = builder.Build(temp.Path, options);

		Assert.DoesNotContain(result.Root.Children, child => child.Name == ".cache");
	}

	// Verifies smart-ignored folders are excluded.
	[Fact]
	public void Build_RespectsSmartIgnoredFolders()
	{
		using var temp = new TemporaryDirectory();
		temp.CreateFile("ignored/skip.txt", "skip");
		temp.CreateFile("keep/ok.txt", "ok");

		var options = new TreeFilterOptions(
			AllowedExtensions: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".txt" },
			AllowedRootFolders: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ignored", "keep" },
			IgnoreRules: new IgnoreRules(false, false, false, false, false, false,
				new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ignored" },
				new HashSet<string>()));

		var builder = new TreeBuilder();
		var result = builder.Build(temp.Path, options);

		Assert.DoesNotContain(result.Root.Children, child => child.Name == "ignored");
		Assert.Contains(result.Root.Children, child => child.Name == "keep");
	}

	// Verifies allowed extensions matching is case-insensitive.
	[Fact]
	public void Build_RespectsAllowedExtensionsCaseInsensitive()
	{
		using var temp = new TemporaryDirectory();
		temp.CreateFile("note.txt", "note");

		var options = new TreeFilterOptions(
			AllowedExtensions: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".TXT" },
			AllowedRootFolders: new HashSet<string>(StringComparer.OrdinalIgnoreCase),
			IgnoreRules: new IgnoreRules(false, false, false, false, false, false, new HashSet<string>(), new HashSet<string>()));

		var builder = new TreeBuilder();
		var result = builder.Build(temp.Path, options);

		Assert.Contains(result.Root.Children, child => child.Name == "note.txt");
	}
}
