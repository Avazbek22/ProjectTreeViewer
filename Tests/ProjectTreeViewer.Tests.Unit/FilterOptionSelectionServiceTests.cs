using System;
using System.Collections.Generic;
using System.Linq;
using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Kernel.Models;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit;

public sealed class FilterOptionSelectionServiceTests
{
	// Verifies default extensions are pre-selected when there are no prior selections.
	[Fact]
	public void BuildExtensionOptions_SelectsDefaultsWhenNoPrevious()
	{
		var service = new FilterOptionSelectionService();

		var options = service.BuildExtensionOptions(new[] { ".txt", ".cs", ".sln" }, new HashSet<string>());

		Assert.Equal(3, options.Count);
		Assert.True(options.Single(o => o.Name == ".cs").IsChecked);
		Assert.True(options.Single(o => o.Name == ".sln").IsChecked);
		Assert.False(options.Single(o => o.Name == ".txt").IsChecked);
	}

	// Verifies prior selections override default extension choices.
	[Fact]
	public void BuildExtensionOptions_RespectsPreviousSelections()
	{
		var service = new FilterOptionSelectionService();
		var previous = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".txt" };

		var options = service.BuildExtensionOptions(new[] { ".txt", ".cs" }, previous);

		Assert.True(options.Single(o => o.Name == ".txt").IsChecked);
		Assert.False(options.Single(o => o.Name == ".cs").IsChecked);
	}

	// Verifies ignored folders are not pre-selected when no prior selections exist.
	[Fact]
	public void BuildRootFolderOptions_ExcludesIgnoredWhenNoPrevious()
	{
		var service = new FilterOptionSelectionService();
		var rules = new IgnoreRules(
			IgnoreBinFolders: true,
			IgnoreObjFolders: true,
			IgnoreHiddenFolders: false,
			IgnoreHiddenFiles: false,
			IgnoreDotFolders: true,
			IgnoreDotFiles: false,
			SmartIgnoredFolders: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "logs" },
			SmartIgnoredFiles: new HashSet<string>());

		var options = service.BuildRootFolderOptions(new[] { "bin", "obj", "logs", ".cache", "src" }, new HashSet<string>(), rules);

		Assert.False(options.Single(o => o.Name == "bin").IsChecked);
		Assert.False(options.Single(o => o.Name == "obj").IsChecked);
		Assert.False(options.Single(o => o.Name == "logs").IsChecked);
		Assert.False(options.Single(o => o.Name == ".cache").IsChecked);
		Assert.True(options.Single(o => o.Name == "src").IsChecked);
	}

	// Verifies explicit previous folder selections are honored.
	[Fact]
	public void BuildRootFolderOptions_RespectsPreviousSelections()
	{
		var service = new FilterOptionSelectionService();
		var rules = new IgnoreRules(
			IgnoreBinFolders: true,
			IgnoreObjFolders: true,
			IgnoreHiddenFolders: false,
			IgnoreHiddenFiles: false,
			IgnoreDotFolders: true,
			IgnoreDotFiles: false,
			SmartIgnoredFolders: new HashSet<string>(),
			SmartIgnoredFiles: new HashSet<string>());
		var previous = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bin" };

		var options = service.BuildRootFolderOptions(new[] { "bin", "src" }, previous, rules);

		Assert.True(options.Single(o => o.Name == "bin").IsChecked);
		Assert.False(options.Single(o => o.Name == "src").IsChecked);
	}
}
