using System;
using System.Linq;
using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Tests.Unit.Helpers;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit;

public sealed class SelectedContentExportServiceTests
{
	// Verifies missing or empty files are ignored when exporting content.
	[Fact]
	public void Build_SkipsMissingAndEmptyFiles()
	{
		using var temp = new TemporaryDirectory();
		var empty = temp.CreateFile("empty.txt", string.Empty);
		var valid = temp.CreateFile("note.txt", "hello");
		var missing = System.IO.Path.Combine(temp.Path, "missing.txt");

		var service = new SelectedContentExportService();
		var result = service.Build(new[] { missing, empty, valid });

		Assert.Contains("note.txt:", result);
		Assert.DoesNotContain("empty.txt", result);
		Assert.DoesNotContain("missing.txt", result);
	}

	// Verifies binary files are excluded from clipboard content.
	[Fact]
	public void Build_SkipsBinaryFiles()
	{
		using var temp = new TemporaryDirectory();
		var binary = temp.CreateBinaryFile("bin.dat", new byte[] { 0, 1, 2, 3 });

		var service = new SelectedContentExportService();
		var result = service.Build(new[] { binary });

		Assert.Equal(string.Empty, result);
	}

	// Verifies exported content is ordered by file path.
	[Fact]
	public void Build_WritesFilesInSortedOrder()
	{
		using var temp = new TemporaryDirectory();
		var fileB = temp.CreateFile("b.txt", "b");
		var fileA = temp.CreateFile("a.txt", "a");

		var service = new SelectedContentExportService();
		var result = service.Build(new[] { fileB, fileA });

		var firstIndex = result.IndexOf("a.txt:", StringComparison.Ordinal);
		var secondIndex = result.IndexOf("b.txt:", StringComparison.Ordinal);
		Assert.True(firstIndex < secondIndex);
	}

	// Verifies whitespace-only file content is treated as empty.
	[Fact]
	public void Build_SkipsWhitespaceOnlyFiles()
	{
		using var temp = new TemporaryDirectory();
		var whitespace = temp.CreateFile("space.txt", "   ");

		var service = new SelectedContentExportService();
		var result = service.Build(new[] { whitespace });

		Assert.Equal(string.Empty, result);
	}

	// Verifies duplicate file paths are included once.
	[Fact]
	public void Build_DeduplicatesPaths()
	{
		using var temp = new TemporaryDirectory();
		var file = temp.CreateFile("dup.txt", "content");

		var service = new SelectedContentExportService();
		var result = service.Build(new[] { file, file });

		Assert.Equal(1, result.Split("dup.txt:").Length - 1);
	}
}
