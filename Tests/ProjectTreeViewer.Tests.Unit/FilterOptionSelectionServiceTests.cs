using System;
using System.Collections.Generic;
using System.Linq;
using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Kernel.Models;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit;

public sealed class FilterOptionSelectionServiceTests
{
	[Fact]
	public void BuildExtensionOptions_SelectsDefaultsWhenNoPrevious()
	{
		var service = new FilterOptionSelectionService();

		var options = service.BuildExtensionOptions(new[] { ".txt", ".cs", ".sln" }, new HashSet<string>());

		Assert.Equal(3, options.Count);
		Assert.True(options.Single(o => o.Value == ".cs").IsSelected);
		Assert.True(options.Single(o => o.Value == ".sln").IsSelected);
		Assert.False(options.Single(o => o.Value == ".txt").IsSelected);
	}

	[Fact]
	public void BuildExtensionOptions_RespectsPreviousSelections()
	{
		var service = new FilterOptionSelectionService();
		var previous = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".txt" };

		var options = service.BuildExtensionOptions(new[] { ".txt", ".cs" }, previous);

		Assert.True(options.Single(o => o.Value == ".txt").IsSelected);
		Assert.False(options.Single(o => o.Value == ".cs").IsSelected);
	}

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

		Assert.False(options.Single(o => o.Value == "bin").IsSelected);
		Assert.False(options.Single(o => o.Value == "obj").IsSelected);
		Assert.False(options.Single(o => o.Value == "logs").IsSelected);
		Assert.False(options.Single(o => o.Value == ".cache").IsSelected);
		Assert.True(options.Single(o => o.Value == "src").IsSelected);
	}

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

		Assert.True(options.Single(o => o.Value == "bin").IsSelected);
		Assert.False(options.Single(o => o.Value == "src").IsSelected);
	}
}
