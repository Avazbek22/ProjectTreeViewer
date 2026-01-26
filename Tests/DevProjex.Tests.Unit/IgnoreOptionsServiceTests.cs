using System.Collections.Generic;
using DevProjex.Application.Services;
using DevProjex.Kernel.Models;
using DevProjex.Tests.Unit.Helpers;
using Xunit;

namespace DevProjex.Tests.Unit;

public sealed class IgnoreOptionsServiceTests
{
	// Verifies ignore options are localized and default-selected flags are set.
	[Fact]
	public void GetOptions_ReturnsLocalizedOptions()
	{
		var catalog = new StubLocalizationCatalog(new Dictionary<AppLanguage, IReadOnlyDictionary<string, string>>
		{
			[AppLanguage.En] = new Dictionary<string, string>
			{
				["Settings.Ignore.BinFolders"] = "Bin",
				["Settings.Ignore.ObjFolders"] = "Obj",
				["Settings.Ignore.HiddenFolders"] = "HiddenFolders",
				["Settings.Ignore.HiddenFiles"] = "HiddenFiles",
				["Settings.Ignore.DotFolders"] = "DotFolders",
				["Settings.Ignore.DotFiles"] = "DotFiles"
			}
		});
		var localization = new LocalizationService(catalog, AppLanguage.En);
		var service = new IgnoreOptionsService(localization);

		var options = service.GetOptions();

		Assert.Equal(6, options.Count);
		Assert.All(options, option => Assert.True(option.DefaultChecked));
		Assert.Contains(options, option => option.Id == IgnoreOptionId.BinFolders && option.Label == "Bin");
		Assert.Contains(options, option => option.Id == IgnoreOptionId.DotFiles && option.Label == "DotFiles");
	}

	// Verifies options preserve the expected ordering.
	[Fact]
	public void GetOptions_ReturnsExpectedOrder()
	{
		var catalog = new StubLocalizationCatalog(new Dictionary<AppLanguage, IReadOnlyDictionary<string, string>>
		{
			[AppLanguage.En] = new Dictionary<string, string>
			{
				["Settings.Ignore.BinFolders"] = "Bin",
				["Settings.Ignore.ObjFolders"] = "Obj",
				["Settings.Ignore.HiddenFolders"] = "HiddenFolders",
				["Settings.Ignore.HiddenFiles"] = "HiddenFiles",
				["Settings.Ignore.DotFolders"] = "DotFolders",
				["Settings.Ignore.DotFiles"] = "DotFiles"
			}
		});
		var localization = new LocalizationService(catalog, AppLanguage.En);
		var service = new IgnoreOptionsService(localization);

		var options = service.GetOptions();

		Assert.Equal(IgnoreOptionId.BinFolders, options[0].Id);
		Assert.Equal(IgnoreOptionId.ObjFolders, options[1].Id);
		Assert.Equal(IgnoreOptionId.HiddenFolders, options[2].Id);
		Assert.Equal(IgnoreOptionId.HiddenFiles, options[3].Id);
		Assert.Equal(IgnoreOptionId.DotFolders, options[4].Id);
		Assert.Equal(IgnoreOptionId.DotFiles, options[5].Id);
	}

	// Verifies localized labels are populated for all options.
	[Fact]
	public void GetOptions_UsesLocalizationForEveryLabel()
	{
		var catalog = new StubLocalizationCatalog(new Dictionary<AppLanguage, IReadOnlyDictionary<string, string>>
		{
			[AppLanguage.En] = new Dictionary<string, string>
			{
				["Settings.Ignore.BinFolders"] = "Bin",
				["Settings.Ignore.ObjFolders"] = "Obj",
				["Settings.Ignore.HiddenFolders"] = "HiddenFolders",
				["Settings.Ignore.HiddenFiles"] = "HiddenFiles",
				["Settings.Ignore.DotFolders"] = "DotFolders",
				["Settings.Ignore.DotFiles"] = "DotFiles"
			}
		});
		var localization = new LocalizationService(catalog, AppLanguage.En);
		var service = new IgnoreOptionsService(localization);

		var options = service.GetOptions();

		Assert.All(options, option => Assert.False(string.IsNullOrWhiteSpace(option.Label)));
	}
}
