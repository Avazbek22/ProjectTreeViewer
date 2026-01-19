using System.Collections.Generic;
using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Kernel.Models;
using ProjectTreeViewer.Tests.Unit.Helpers;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit;

public sealed class LocalizationServiceTests
{
	[Fact]
	public void Indexer_ReturnsLocalizedValue()
	{
		var catalog = new StubLocalizationCatalog(new Dictionary<AppLanguage, IReadOnlyDictionary<string, string>>
		{
			[AppLanguage.En] = new Dictionary<string, string> { ["Greeting"] = "Hello" }
		});
		var service = new LocalizationService(catalog, AppLanguage.En);

		Assert.Equal("Hello", service["Greeting"]);
	}

	[Fact]
	public void Indexer_ReturnsFallbackForMissingKey()
	{
		var catalog = new StubLocalizationCatalog(new Dictionary<AppLanguage, IReadOnlyDictionary<string, string>>
		{
			[AppLanguage.En] = new Dictionary<string, string>()
		});
		var service = new LocalizationService(catalog, AppLanguage.En);

		Assert.Equal("[[Missing]]", service["Missing"]);
	}

	[Fact]
	public void Format_UsesStringFormat()
	{
		var catalog = new StubLocalizationCatalog(new Dictionary<AppLanguage, IReadOnlyDictionary<string, string>>
		{
			[AppLanguage.En] = new Dictionary<string, string> { ["Format"] = "Hello {0}" }
		});
		var service = new LocalizationService(catalog, AppLanguage.En);

		Assert.Equal("Hello World", service.Format("Format", "World"));
	}

	[Fact]
	public void SetLanguage_RaisesEventWhenChanged()
	{
		var catalog = new StubLocalizationCatalog(new Dictionary<AppLanguage, IReadOnlyDictionary<string, string>>
		{
			[AppLanguage.En] = new Dictionary<string, string>(),
			[AppLanguage.Ru] = new Dictionary<string, string>()
		});
		var service = new LocalizationService(catalog, AppLanguage.En);
		int called = 0;
		service.LanguageChanged += (_, _) => called++;

		service.SetLanguage(AppLanguage.Ru);

		Assert.Equal(1, called);
		Assert.Equal(AppLanguage.Ru, service.CurrentLanguage);
	}

	[Fact]
	public void SetLanguage_DoesNothingWhenSame()
	{
		var catalog = new StubLocalizationCatalog(new Dictionary<AppLanguage, IReadOnlyDictionary<string, string>>
		{
			[AppLanguage.En] = new Dictionary<string, string>()
		});
		var service = new LocalizationService(catalog, AppLanguage.En);
		int called = 0;
		service.LanguageChanged += (_, _) => called++;

		service.SetLanguage(AppLanguage.En);

		Assert.Equal(0, called);
	}
}
