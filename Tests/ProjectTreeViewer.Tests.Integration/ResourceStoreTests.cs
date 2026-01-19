using System.Collections.Generic;
using ProjectTreeViewer.Infrastructure.ResourceStore;
using ProjectTreeViewer.Kernel.Models;
using Xunit;

namespace ProjectTreeViewer.Tests.Integration;

public sealed class ResourceStoreTests
{
	[Fact]
	public void JsonLocalizationCatalog_LoadsResources()
	{
		var catalog = new JsonLocalizationCatalog();
		var dict = catalog.Get(AppLanguage.En);

		Assert.Contains("Settings.Ignore.BinFolders", dict.Keys);
	}

	[Fact]
	public void EmbeddedIconStore_ReturnsKnownIconBytes()
	{
		var store = new EmbeddedIconStore();

		Assert.Contains("folder", store.Keys);
		var bytes = store.GetIconBytes("folder");
		Assert.NotEmpty(bytes);
	}

	[Fact]
	public void EmbeddedIconStore_ThrowsForMissingKey()
	{
		var store = new EmbeddedIconStore();

		Assert.Throws<KeyNotFoundException>(() => store.GetIconBytes("missing"));
	}

	[Fact]
	public void IconMapper_MapsKnownExtensions()
	{
		var mapper = new IconMapper();
		var node = new FileSystemNode("Program.cs", "/root/Program.cs", false, false, new List<FileSystemNode>());

		var key = mapper.GetIconKey(node);

		Assert.Equal("csharp", key);
	}

	[Fact]
	public void IconMapper_MapsGrayFolders()
	{
		var mapper = new IconMapper();
		var node = new FileSystemNode("bin", "/root/bin", true, false, new List<FileSystemNode>());

		var key = mapper.GetIconKey(node);

		Assert.Equal("grayFolder", key);
	}
}
