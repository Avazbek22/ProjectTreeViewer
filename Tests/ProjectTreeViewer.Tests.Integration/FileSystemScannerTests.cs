using System.Collections.Generic;
using ProjectTreeViewer.Infrastructure.FileSystem;
using ProjectTreeViewer.Kernel.Models;
using ProjectTreeViewer.Tests.Integration.Helpers;
using Xunit;

namespace ProjectTreeViewer.Tests.Integration;

public sealed class FileSystemScannerTests
{
	// Verifies scanning returns extensions from files across the tree.
	[Fact]
	public void GetExtensions_ReturnsExtensionsFromTree()
	{
		using var temp = new TemporaryDirectory();
		temp.CreateFile("src/app.cs", "class A {}");
		temp.CreateFile("src/readme.md", "hello");
		temp.CreateFile("root.txt", "root");

		var scanner = new FileSystemScanner();
		var rules = new IgnoreRules(false, false, false, false, false, false, new HashSet<string>(), new HashSet<string>());

		var result = scanner.GetExtensions(temp.Path, rules);

		Assert.Contains(".cs", result.Value);
		Assert.Contains(".md", result.Value);
		Assert.Contains(".txt", result.Value);
		Assert.False(result.RootAccessDenied);
		Assert.False(result.HadAccessDenied);
	}

	// Verifies ignore rules exclude hidden and dot files.
	[Fact]
	public void GetExtensions_RespectsIgnoreRules()
	{
		using var temp = new TemporaryDirectory();
		temp.CreateFile(".hidden", "hidden");
		temp.CreateFile("visible.txt", "visible");

		var scanner = new FileSystemScanner();
		var rules = new IgnoreRules(false, false, false, true, false, true, new HashSet<string>(), new HashSet<string>());

		var result = scanner.GetExtensions(temp.Path, rules);

		Assert.DoesNotContain(string.Empty, result.Value);
		Assert.Single(result.Value);
		Assert.Contains(".txt", result.Value);
	}

	// Verifies root file extension scanning ignores nested files.
	[Fact]
	public void GetRootFileExtensions_ReturnsOnlyRootFiles()
	{
		using var temp = new TemporaryDirectory();
		temp.CreateFile("root.cs", "class A {}");
		temp.CreateFile("src/nested.txt", "nested");

		var scanner = new FileSystemScanner();
		var rules = new IgnoreRules(false, false, false, false, false, false, new HashSet<string>(), new HashSet<string>());

		var result = scanner.GetRootFileExtensions(temp.Path, rules);

		Assert.Contains(".cs", result.Value);
		Assert.DoesNotContain(".txt", result.Value);
	}

	// Verifies ignore rules filter root folder names.
	[Fact]
	public void GetRootFolderNames_RespectsIgnoreRules()
	{
		using var temp = new TemporaryDirectory();
		temp.CreateDirectory("bin");
		temp.CreateDirectory("src");

		var scanner = new FileSystemScanner();
		var rules = new IgnoreRules(true, false, false, false, false, false, new HashSet<string>(), new HashSet<string>());

		var result = scanner.GetRootFolderNames(temp.Path, rules);

		Assert.DoesNotContain("bin", result.Value);
		Assert.Contains("src", result.Value);
	}

	// Verifies CanReadRoot returns true for accessible directories.
	[Fact]
	public void CanReadRoot_ReturnsTrueForExistingFolder()
	{
		using var temp = new TemporaryDirectory();
		var scanner = new FileSystemScanner();

		Assert.True(scanner.CanReadRoot(temp.Path));
	}

	// Verifies scanner gracefully handles a missing root directory.
	[Fact]
	public void GetExtensions_ReturnsEmptyForMissingRoot()
	{
		var scanner = new FileSystemScanner();
		var rules = new IgnoreRules(false, false, false, false, false, false, new HashSet<string>(), new HashSet<string>());

		var result = scanner.GetExtensions("/path/does/not/exist", rules);

		Assert.Empty(result.Value);
		Assert.False(result.RootAccessDenied);
		Assert.False(result.HadAccessDenied);
	}
}
