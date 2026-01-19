using System;
using System.Collections.Generic;
using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Kernel.Models;
using ProjectTreeViewer.Tests.Unit.Helpers;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit;

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
}
