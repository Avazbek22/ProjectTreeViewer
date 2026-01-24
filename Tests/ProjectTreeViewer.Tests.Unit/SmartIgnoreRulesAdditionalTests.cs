using System;
using System.Collections.Generic;
using System.IO;
using ProjectTreeViewer.Infrastructure.SmartIgnore;
using ProjectTreeViewer.Tests.Unit.Helpers;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit;

public sealed class SmartIgnoreRulesAdditionalTests
{
	[Theory]
	// Verifies common smart ignore rule includes expected folder names.
	[InlineData(".git")]
	[InlineData(".svn")]
	[InlineData(".hg")]
	[InlineData(".vs")]
	[InlineData(".idea")]
	[InlineData(".vscode")]
	[InlineData("node_modules")]
	public void CommonSmartIgnoreRule_IncludesDefaultFolders(string folderName)
	{
		var rule = new CommonSmartIgnoreRule();

		var result = rule.Evaluate("any");

		Assert.Contains(folderName, result.Folders, StringComparer.OrdinalIgnoreCase);
	}

	[Theory]
	// Verifies common smart ignore rule includes expected file names.
	[InlineData(".ds_store")]
	[InlineData("thumbs.db")]
	[InlineData("desktop.ini")]
	public void CommonSmartIgnoreRule_IncludesDefaultFiles(string fileName)
	{
		var rule = new CommonSmartIgnoreRule();

		var result = rule.Evaluate("any");

		Assert.Contains(fileName, result.Files, StringComparer.OrdinalIgnoreCase);
	}

	[Theory]
	// Verifies frontend artifacts rule returns empty sets without marker files.
	[InlineData("readme.md")]
	[InlineData("package.txt")]
	[InlineData("yarn.json")]
	[InlineData("lockfile")]
	public void FrontendArtifactsIgnoreRule_NoMarkers_ReturnsEmpty(string fileName)
	{
		using var temp = new TemporaryDirectory();
		var rule = new FrontendArtifactsIgnoreRule();
		temp.CreateFile(fileName, "content");

		var result = rule.Evaluate(temp.Path);

		Assert.Empty(result.Folders);
		Assert.Empty(result.Files);
	}

	[Theory]
	// Verifies frontend artifacts rule activates when marker files are present.
	[InlineData("package.json")]
	[InlineData("package-lock.json")]
	[InlineData("pnpm-lock.yaml")]
	[InlineData("yarn.lock")]
	public void FrontendArtifactsIgnoreRule_WithMarker_IncludesFolders(string markerFile)
	{
		using var temp = new TemporaryDirectory();
		var rule = new FrontendArtifactsIgnoreRule();
		temp.CreateFile(markerFile, "content");

		var result = rule.Evaluate(temp.Path);

		Assert.Contains("dist", result.Folders, StringComparer.OrdinalIgnoreCase);
		Assert.Contains("build", result.Folders, StringComparer.OrdinalIgnoreCase);
		Assert.Contains(".next", result.Folders, StringComparer.OrdinalIgnoreCase);
		Assert.Contains(".nuxt", result.Folders, StringComparer.OrdinalIgnoreCase);
		Assert.Contains(".turbo", result.Folders, StringComparer.OrdinalIgnoreCase);
		Assert.Contains(".svelte-kit", result.Folders, StringComparer.OrdinalIgnoreCase);
	}
}
