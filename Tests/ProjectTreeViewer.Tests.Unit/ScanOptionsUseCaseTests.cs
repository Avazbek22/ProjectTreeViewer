using System;
using System.Collections.Generic;
using ProjectTreeViewer.Application.UseCases;
using ProjectTreeViewer.Kernel.Contracts;
using ProjectTreeViewer.Kernel.Models;
using ProjectTreeViewer.Tests.Unit.Helpers;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit;

public sealed class ScanOptionsUseCaseTests
{
	[Fact]
	public void Execute_SortsResultsAndCombinesAccessFlags()
	{
		var scanner = new StubFileSystemScanner
		{
			GetExtensionsHandler = (_, _) => new ScanResult<HashSet<string>>(
				new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".b", ".a" },
				rootAccessDenied: true,
				hadAccessDenied: false),
			GetRootFolderNamesHandler = (_, _) => new ScanResult<List<string>>(
				new List<string> { "z", "a" },
				rootAccessDenied: false,
				hadAccessDenied: true)
		};

		var useCase = new ScanOptionsUseCase(scanner);
		var result = useCase.Execute(new ScanOptionsRequest("/root", new IgnoreRules(
			IgnoreBinFolders: false,
			IgnoreObjFolders: false,
			IgnoreHiddenFolders: false,
			IgnoreHiddenFiles: false,
			IgnoreDotFolders: false,
			IgnoreDotFiles: false,
			SmartIgnoredFolders: new HashSet<string>(),
			SmartIgnoredFiles: new HashSet<string>())));

		Assert.Equal(new[] { ".a", ".b" }, result.Extensions);
		Assert.Equal(new[] { "a", "z" }, result.RootFolders);
		Assert.True(result.RootAccessDenied);
		Assert.True(result.HadAccessDenied);
	}

	[Fact]
	public void GetExtensionsForRootFolders_ReturnsRootFilesWhenNoFolders()
	{
		var scanner = new StubFileSystemScanner();
		var useCase = new ScanOptionsUseCase(scanner);

		var result = useCase.GetExtensionsForRootFolders("/root", new List<string>(), new IgnoreRules(
			IgnoreBinFolders: false,
			IgnoreObjFolders: false,
			IgnoreHiddenFolders: false,
			IgnoreHiddenFiles: false,
			IgnoreDotFolders: false,
			IgnoreDotFiles: false,
			SmartIgnoredFolders: new HashSet<string>(),
			SmartIgnoredFiles: new HashSet<string>()));

		Assert.Empty(result.Value);
		Assert.False(result.RootAccessDenied);
		Assert.False(result.HadAccessDenied);
	}

	[Fact]
	public void CanReadRoot_DelegatesToScanner()
	{
		var scanner = new StubFileSystemScanner
		{
			CanReadRootHandler = _ => false
		};
		var useCase = new ScanOptionsUseCase(scanner);

		Assert.False(useCase.CanReadRoot("/root"));
	}
}
