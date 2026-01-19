using System.Collections.Generic;
using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Kernel.Models;
using ProjectTreeViewer.Tests.Unit.Helpers;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit;

public sealed class IgnoreOptionsServiceTests
{
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
}
