using System;
using System.Collections.Generic;
using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Kernel.Models;
using ProjectTreeViewer.Tests.Unit.Helpers;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit;

public sealed class SmartIgnoreServiceTests
{
	[Fact]
	public void Build_MergesRuleResults()
	{
		var rules = new[]
		{
			new StubSmartIgnoreRule(new SmartIgnoreResult(
				new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bin" },
				new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "file.tmp" })),
			new StubSmartIgnoreRule(new SmartIgnoreResult(
				new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "obj" },
				new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "file.tmp" }))
		};

		var service = new SmartIgnoreService(rules);

		var result = service.Build("/root");

		Assert.Contains("bin", result.FolderNames);
		Assert.Contains("obj", result.FolderNames);
		Assert.Single(result.FileNames);
	}
}
