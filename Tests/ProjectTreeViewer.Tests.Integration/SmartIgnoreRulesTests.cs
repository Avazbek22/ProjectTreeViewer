using ProjectTreeViewer.Infrastructure.SmartIgnore;
using ProjectTreeViewer.Tests.Integration.Helpers;
using Xunit;

namespace ProjectTreeViewer.Tests.Integration;

public sealed class SmartIgnoreRulesTests
{
	[Fact]
	public void CommonSmartIgnoreRule_ReturnsKnownEntries()
	{
		var rule = new CommonSmartIgnoreRule();
		var result = rule.Evaluate("/root");

		Assert.Contains(".git", result.FolderNames);
		Assert.Contains("thumbs.db", result.FileNames);
	}

	[Fact]
	public void FrontendArtifactsIgnoreRule_ReturnsEmptyWhenNoMarkers()
	{
		using var temp = new TemporaryDirectory();
		var rule = new FrontendArtifactsIgnoreRule();

		var result = rule.Evaluate(temp.Path);

		Assert.Empty(result.FolderNames);
	}

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
