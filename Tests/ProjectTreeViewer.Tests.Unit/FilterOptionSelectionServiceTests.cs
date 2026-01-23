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

	// Verifies extension options are sorted by name using an ordinal ignore-case comparer.
	[Fact]
	public void BuildExtensionOptions_SortsExtensionsCaseInsensitive()
	{
		var service = new FilterOptionSelectionService();

		var options = service.BuildExtensionOptions(new[] { ".B", ".a" }, new HashSet<string>());

		Assert.Equal(".a", options[0].Name);
		Assert.Equal(".B", options[1].Name);
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

	// Verifies designer extensions are included in the default selections.
	[Fact]
	public void BuildExtensionOptions_SelectsDesignerByDefault()
	{
		var service = new FilterOptionSelectionService();

		var options = service.BuildExtensionOptions(new[] { ".designer", ".txt" }, new HashSet<string>());

		Assert.True(options.Single(o => o.Name == ".designer").IsChecked);
		Assert.False(options.Single(o => o.Name == ".txt").IsChecked);
	}

	// Verifies previous selections are applied case-insensitively.
	[Fact]
	public void BuildExtensionOptions_RespectsPreviousSelectionsCaseInsensitive()
	{
		var service = new FilterOptionSelectionService();
		var previous = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".CS" };

		var options = service.BuildExtensionOptions(new[] { ".cs" }, previous);

		Assert.True(options.Single(o => o.Name == ".cs").IsChecked);
	}

	// Verifies explicit selection keeps ignored folders checked.
	[Fact]
	public void BuildRootFolderOptions_RespectsExplicitSelectionEvenIfIgnored()
	{
		var service = new FilterOptionSelectionService();
		var rules = new IgnoreRules(
			IgnoreBinFolders: true,
			IgnoreObjFolders: false,
			IgnoreHiddenFolders: false,
			IgnoreHiddenFiles: false,
			IgnoreDotFolders: false,
			IgnoreDotFiles: false,
			SmartIgnoredFolders: new HashSet<string>(),
			SmartIgnoredFiles: new HashSet<string>());
		var previous = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bin" };

		var options = service.BuildRootFolderOptions(new[] { "bin", "src" }, previous, rules);

		Assert.True(options.Single(o => o.Name == "bin").IsChecked);
	}

	// Verifies root folder options preserve the input order.
	[Fact]
	public void BuildRootFolderOptions_PreservesInputOrder()
	{
		var service = new FilterOptionSelectionService();
		var rules = new IgnoreRules(
			IgnoreBinFolders: false,
			IgnoreObjFolders: false,
			IgnoreHiddenFolders: false,
			IgnoreHiddenFiles: false,
			IgnoreDotFolders: false,
			IgnoreDotFiles: false,
			SmartIgnoredFolders: new HashSet<string>(),
			SmartIgnoredFiles: new HashSet<string>());

		var options = service.BuildRootFolderOptions(new[] { "b", "a" }, new HashSet<string>(), rules);

		Assert.Equal("b", options[0].Name);
		Assert.Equal("a", options[1].Name);
	}
}
