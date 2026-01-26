using System;
using System.Collections.Generic;
using DevProjex.Application.Services;
using DevProjex.Kernel.Abstractions;
using DevProjex.Kernel.Models;
using DevProjex.Tests.Unit.Helpers;
using Xunit;

namespace DevProjex.Tests.Unit;

public sealed class IgnoreRulesServiceTests
{
	// Verifies selected ignore options and smart-ignore rules are merged into IgnoreRules.
	[Fact]
	public void Build_CombinesSelectedOptionsAndSmartIgnore()
	{
		var smartResult = new SmartIgnoreResult(
			new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "cache" },
			new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "thumbs.db" });
		var smart = new SmartIgnoreService(new[] { new StubSmartIgnoreRule(smartResult) });
		var service = new IgnoreRulesService(smart);

		var rules = service.Build("/root", new[] { IgnoreOptionId.BinFolders, IgnoreOptionId.DotFiles });

		Assert.True(rules.IgnoreBinFolders);
		Assert.True(rules.IgnoreDotFiles);
		Assert.False(rules.IgnoreObjFolders);
		Assert.Contains("cache", rules.SmartIgnoredFolders);
		Assert.Contains("thumbs.db", rules.SmartIgnoredFiles);
	}

	// Verifies no selections keep ignore flags disabled.
	[Fact]
	public void Build_ReturnsAllFlagsFalseWhenNoSelections()
	{
		var smart = new SmartIgnoreService(new ISmartIgnoreRule[0]);
		var service = new IgnoreRulesService(smart);

		var rules = service.Build("/root", Array.Empty<IgnoreOptionId>());

		Assert.False(rules.IgnoreBinFolders);
		Assert.False(rules.IgnoreObjFolders);
		Assert.False(rules.IgnoreHiddenFolders);
		Assert.False(rules.IgnoreHiddenFiles);
		Assert.False(rules.IgnoreDotFolders);
		Assert.False(rules.IgnoreDotFiles);
	}

	// Verifies smart-ignore results are case-insensitive.
	[Fact]
	public void Build_MergesSmartIgnoreCaseInsensitive()
	{
		var smartResult = new SmartIgnoreResult(
			new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Cache" },
			new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Thumbs.DB" });
		var smart = new SmartIgnoreService(new[] { new StubSmartIgnoreRule(smartResult) });
		var service = new IgnoreRulesService(smart);

		var rules = service.Build("/root", Array.Empty<IgnoreOptionId>());

		Assert.Contains("cache", rules.SmartIgnoredFolders);
		Assert.Contains("thumbs.db", rules.SmartIgnoredFiles);
	}

	// Verifies all selected options enable all ignore flags.
	[Fact]
	public void Build_SetsAllFlagsWhenAllOptionsSelected()
	{
		var smart = new SmartIgnoreService(new ISmartIgnoreRule[0]);
		var service = new IgnoreRulesService(smart);

		var rules = service.Build("/root", new[]
		{
			IgnoreOptionId.BinFolders,
			IgnoreOptionId.ObjFolders,
			IgnoreOptionId.HiddenFolders,
			IgnoreOptionId.HiddenFiles,
			IgnoreOptionId.DotFolders,
			IgnoreOptionId.DotFiles
		});

		Assert.True(rules.IgnoreBinFolders);
		Assert.True(rules.IgnoreObjFolders);
		Assert.True(rules.IgnoreHiddenFolders);
		Assert.True(rules.IgnoreHiddenFiles);
		Assert.True(rules.IgnoreDotFolders);
		Assert.True(rules.IgnoreDotFiles);
	}

	// Verifies smart-ignore results are merged even when no selections.
	[Fact]
	public void Build_UsesSmartIgnoreWhenNoSelections()
	{
		var smartResult = new SmartIgnoreResult(
			new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "cache" },
			new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "thumbs.db" });
		var smart = new SmartIgnoreService(new[] { new StubSmartIgnoreRule(smartResult) });
		var service = new IgnoreRulesService(smart);

		var rules = service.Build("/root", Array.Empty<IgnoreOptionId>());

		Assert.Contains("cache", rules.SmartIgnoredFolders);
		Assert.Contains("thumbs.db", rules.SmartIgnoredFiles);
	}
}
