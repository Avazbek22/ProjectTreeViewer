using System.Globalization;
using ProjectTreeViewer.Kernel.Models;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit;

public sealed class CommandLineOptionsTests
{
	[Fact]
	public void Parse_ReturnsEmptyWhenNoArgs()
	{
		var result = CommandLineOptions.Parse(Array.Empty<string>());

		Assert.Equal(CommandLineOptions.Empty, result);
	}

	[Fact]
	public void Parse_ReadsPathLanguageAndElevation()
	{
		var result = CommandLineOptions.Parse(new[] { "--path", "/tmp/root", "--lang", "ru", "--elevationAttempted" });

		Assert.Equal("/tmp/root", result.Path);
		Assert.Equal(AppLanguage.Ru, result.Language);
		Assert.True(result.ElevationAttempted);
	}

	[Fact]
	public void Parse_IgnoresUnknownArgs()
	{
		var result = CommandLineOptions.Parse(new[] { "--unknown", "value" });

		Assert.Equal(CommandLineOptions.Empty, result);
	}

	[Fact]
	public void WithElevationAttempted_SetsFlag()
	{
		var result = CommandLineOptions.Empty.WithElevationAttempted();

		Assert.True(result.ElevationAttempted);
	}

	[Fact]
	public void ToArguments_QuotesPathsWithSpaces()
	{
		var options = new CommandLineOptions("/tmp/root folder", AppLanguage.En, true);

		var args = options.ToArguments();

		Assert.Contains("--path", args);
		Assert.Contains("\"/tmp/root folder\"", args);
		Assert.Contains("--lang", args);
		Assert.Contains("en", args);
		Assert.Contains("--elevationAttempted", args);
	}

	[Fact]
	public void ParseLanguage_ReturnsNullForUnknown()
	{
		Assert.Null(CommandLineOptions.ParseLanguage("xx"));
	}

	[Fact]
	public void LanguageToCode_UsesEnglishFallback()
	{
		var value = CommandLineOptions.LanguageToCode((AppLanguage)999);

		Assert.Equal("en", value);
	}

	[Fact]
	public void DetectSystemLanguage_ReturnsExpectedForCulture()
	{
		var original = CultureInfo.CurrentUICulture;
		try
		{
			CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

			var detected = CommandLineOptions.DetectSystemLanguage();

			Assert.Equal(AppLanguage.Fr, detected);
		}
		finally
		{
			CultureInfo.CurrentUICulture = original;
		}
	}
}
