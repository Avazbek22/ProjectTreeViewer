using ProjectTreeViewer.Infrastructure.SmartIgnore;
using ProjectTreeViewer.Tests.Integration.Helpers;
using Xunit;

namespace ProjectTreeViewer.Tests.Integration;

public sealed class SmartIgnoreRulesTests
{
	// Verifies the common rule includes standard SCM/temp entries.
	[Fact]
	public void CommonSmartIgnoreRule_ReturnsKnownEntries()
	{
		var rule = new CommonSmartIgnoreRule();
		var result = rule.Evaluate("/root");

		Assert.Contains(".git", result.FolderNames);
		Assert.Contains("thumbs.db", result.FileNames);
	}

	// Verifies frontend ignore rule is empty when no marker files exist.
	[Fact]
	public void FrontendArtifactsIgnoreRule_ReturnsEmptyWhenNoMarkers()
	{
		using var temp = new TemporaryDirectory();
		var rule = new FrontendArtifactsIgnoreRule();

		var result = rule.Evaluate(temp.Path);

		Assert.Empty(result.FolderNames);
	}

	// Verifies frontend ignore rule activates when marker files exist.
	[Fact]
	public void FrontendArtifactsIgnoreRule_ReturnsFoldersWhenMarkerPresent()
	{
		using var temp = new TemporaryDirectory();
		temp.CreateFile("package.json", "{}");

		var rule = new FrontendArtifactsIgnoreRule();
		var result = rule.Evaluate(temp.Path);

		Assert.Contains("dist", result.FolderNames);
		Assert.Contains("build", result.FolderNames);
	}
}
