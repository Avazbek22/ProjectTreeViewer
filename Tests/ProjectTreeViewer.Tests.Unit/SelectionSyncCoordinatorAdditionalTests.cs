using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Application.UseCases;
using ProjectTreeViewer.Avalonia.Coordinators;
using ProjectTreeViewer.Avalonia.ViewModels;
using ProjectTreeViewer.Infrastructure.ResourceStore;
using ProjectTreeViewer.Kernel.Models;
using ProjectTreeViewer.Tests.Unit.Helpers;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit;

public sealed class SelectionSyncCoordinatorAdditionalTests
{
	[Fact]
	public void HandleRootAllChanged_ChecksAllRootFolderOptions()
	{
		var viewModel = CreateViewModel();
		viewModel.RootFolders.Add(new SelectionOptionViewModel("src", false));
		viewModel.RootFolders.Add(new SelectionOptionViewModel("tests", false));

		var coordinator = CreateCoordinator(viewModel);

		coordinator.HandleRootAllChanged(true, currentPath: null);

		Assert.True(viewModel.AllRootFoldersChecked);
		Assert.All(viewModel.RootFolders, option => Assert.True(option.IsChecked));
	}

	[Fact]
	public void HandleExtensionsAllChanged_ChecksAllExtensionOptions()
	{
		var viewModel = CreateViewModel();
		viewModel.Extensions.Add(new SelectionOptionViewModel(".cs", false));
		viewModel.Extensions.Add(new SelectionOptionViewModel(".md", false));

		var coordinator = CreateCoordinator(viewModel);

		coordinator.HandleExtensionsAllChanged(true);

		Assert.True(viewModel.AllExtensionsChecked);
		Assert.All(viewModel.Extensions, option => Assert.True(option.IsChecked));
	}

	[Fact]
	public void HandleIgnoreAllChanged_ChecksAllIgnoreOptions()
	{
		var viewModel = CreateViewModel();
		viewModel.IgnoreOptions.Add(new IgnoreOptionViewModel(IgnoreOptionId.BinFolders, "bin", false));
		viewModel.IgnoreOptions.Add(new IgnoreOptionViewModel(IgnoreOptionId.ObjFolders, "obj", false));

		var coordinator = CreateCoordinator(viewModel);

		coordinator.HandleIgnoreAllChanged(true, currentPath: null);

		Assert.True(viewModel.AllIgnoreChecked);
		Assert.All(viewModel.IgnoreOptions, option => Assert.True(option.IsChecked));
	}

	[Fact]
	public async Task PopulateExtensionsForRootSelectionAsync_EmptyRoots_ClearsExtensions()
	{
		var viewModel = CreateViewModel();
		viewModel.Extensions.Add(new SelectionOptionViewModel(".cs", true));
		viewModel.Extensions.Add(new SelectionOptionViewModel(".md", true));
		viewModel.AllExtensionsChecked = true;

		var coordinator = CreateCoordinator(viewModel);

		await coordinator.PopulateExtensionsForRootSelectionAsync("root", new List<string>());

		Assert.Empty(viewModel.Extensions);
		Assert.False(viewModel.AllExtensionsChecked);
	}

	[Fact]
	public async Task PopulateExtensionsForRootSelectionAsync_EmptyPath_DoesNotChangeExtensions()
	{
		var viewModel = CreateViewModel();
		viewModel.Extensions.Add(new SelectionOptionViewModel(".cs", true));

		var coordinator = CreateCoordinator(viewModel);

		await coordinator.PopulateExtensionsForRootSelectionAsync(string.Empty, new List<string> { "src" });

		Assert.Single(viewModel.Extensions);
		Assert.Equal(".cs", viewModel.Extensions[0].Name);
	}

	[Fact]
	public async Task PopulateRootFoldersAsync_EmptyPath_DoesNotChangeRootFolders()
	{
		var viewModel = CreateViewModel();
		viewModel.RootFolders.Add(new SelectionOptionViewModel("src", true));

		var coordinator = CreateCoordinator(viewModel);

		await coordinator.PopulateRootFoldersAsync(string.Empty);

		Assert.Single(viewModel.RootFolders);
		Assert.Equal("src", viewModel.RootFolders[0].Name);
	}

	[Fact]
	public async Task UpdateLiveOptionsFromRootSelectionAsync_EmptyPath_DoesNotChangeOptions()
	{
		var viewModel = CreateViewModel();
		viewModel.Extensions.Add(new SelectionOptionViewModel(".cs", true));
		viewModel.IgnoreOptions.Add(new IgnoreOptionViewModel(IgnoreOptionId.BinFolders, "bin", true));

		var coordinator = CreateCoordinator(viewModel);

		await coordinator.UpdateLiveOptionsFromRootSelectionAsync(null);

		Assert.Single(viewModel.Extensions);
		Assert.Single(viewModel.IgnoreOptions);
	}

	[Fact]
	public void PopulateExtensionsForRootSelectionAsync_DoesNotDropCachedSelections()
	{
		var viewModel = CreateViewModel();
		viewModel.Extensions.Add(new SelectionOptionViewModel(".cs", false));
		viewModel.Extensions.Add(new SelectionOptionViewModel(".md", true));

		var coordinator = CreateCoordinator(viewModel);
		coordinator.UpdateExtensionsSelectionCache();

		coordinator.ApplyExtensionScan(new[] { ".cs" });
		coordinator.ApplyExtensionScan(new[] { ".cs", ".md" });

		var md = viewModel.Extensions.Single(option => option.Name == ".md");
		Assert.True(md.IsChecked);
	}

	[Fact]
	public void PopulateExtensionsForRootSelectionAsync_EmptyRoots_DoesNotClearCachedSelections()
	{
		var viewModel = CreateViewModel();
		viewModel.Extensions.Add(new SelectionOptionViewModel(".cs", false));
		viewModel.Extensions.Add(new SelectionOptionViewModel(".md", true));

		var coordinator = CreateCoordinator(viewModel);
		coordinator.UpdateExtensionsSelectionCache();

		coordinator.ApplyExtensionScan(Array.Empty<string>());
		coordinator.ApplyExtensionScan(new[] { ".cs", ".md" });

		var md = viewModel.Extensions.Single(option => option.Name == ".md");
		Assert.True(md.IsChecked);
	}

	[Fact]
	public void ApplyExtensionScan_UpdatesExtensionsFromScanResults()
	{
		var viewModel = CreateViewModel();
		viewModel.Extensions.Add(new SelectionOptionViewModel(".old", true));

		var coordinator = CreateCoordinator(viewModel);

		coordinator.ApplyExtensionScan(new[] { ".cs", ".md", ".root" });

		var names = viewModel.Extensions.Select(option => option.Name).ToList();
		Assert.Contains(".root", names);
		Assert.Contains(".cs", names);
		Assert.Contains(".md", names);
		Assert.DoesNotContain(".old", names);
	}

	[Fact]
	public void ApplyExtensionScan_PreservesCachedExtensionSelections()
	{
		var viewModel = CreateViewModel();
		viewModel.Extensions.Add(new SelectionOptionViewModel(".md", true));
		viewModel.Extensions.Add(new SelectionOptionViewModel(".txt", false));
		viewModel.AllExtensionsChecked = false;

		var coordinator = CreateCoordinator(viewModel);
		coordinator.UpdateExtensionsSelectionCache();

		coordinator.ApplyExtensionScan(new[] { ".md", ".txt" });

		var md = viewModel.Extensions.Single(option => option.Name == ".md");
		var txt = viewModel.Extensions.Single(option => option.Name == ".txt");
		Assert.True(md.IsChecked);
		Assert.False(txt.IsChecked);
	}

	[Fact]
	public void ApplyExtensionScan_EmptyScan_ClearsExtensionsAndAllFlag()
	{
		var viewModel = CreateViewModel();
		viewModel.Extensions.Add(new SelectionOptionViewModel(".cs", true));
		viewModel.AllExtensionsChecked = true;

		var coordinator = CreateCoordinator(viewModel);

		coordinator.ApplyExtensionScan(Array.Empty<string>());

		Assert.Empty(viewModel.Extensions);
		Assert.False(viewModel.AllExtensionsChecked);
	}

	[Fact]
	public void PopulateIgnoreOptionsForRootSelection_EmptyRoots_ClearsIgnoreOptionsAndAllFlag()
	{
		var viewModel = CreateViewModel();
		viewModel.IgnoreOptions.Add(new IgnoreOptionViewModel(IgnoreOptionId.BinFolders, "bin", true));
		viewModel.AllIgnoreChecked = true;

		var coordinator = CreateCoordinator(viewModel);

		coordinator.PopulateIgnoreOptionsForRootSelection(Array.Empty<string>());

		Assert.Empty(viewModel.IgnoreOptions);
		Assert.False(viewModel.AllIgnoreChecked);
	}

	[Fact]
	public void PopulateIgnoreOptionsForRootSelection_PreservesIgnoreSelections()
	{
		var viewModel = CreateViewModel();
		var coordinator = CreateCoordinator(viewModel);
		coordinator.PopulateIgnoreOptionsForRootSelection(new[] { "src" });
		coordinator.HandleIgnoreAllChanged(false, currentPath: null);
		viewModel.IgnoreOptions[0].IsChecked = true;
		viewModel.IgnoreOptions[1].IsChecked = false;
		coordinator.UpdateIgnoreSelectionCache();

		coordinator.PopulateIgnoreOptionsForRootSelection(new[] { "src" });

		var bin = viewModel.IgnoreOptions.Single(option => option.Id == IgnoreOptionId.BinFolders);
		var obj = viewModel.IgnoreOptions.Single(option => option.Id == IgnoreOptionId.ObjFolders);
		Assert.True(bin.IsChecked);
		Assert.False(obj.IsChecked);
	}

	private static MainWindowViewModel CreateViewModel()
	{
		var localization = new LocalizationService(CreateCatalog(), AppLanguage.En);
		return new MainWindowViewModel(localization, new HelpContentProvider());
	}

	private static SelectionSyncCoordinator CreateCoordinator(MainWindowViewModel viewModel, StubFileSystemScanner? scanner = null)
	{
		var localization = new LocalizationService(CreateCatalog(), AppLanguage.En);
		scanner ??= new StubFileSystemScanner();
		var scanOptions = new ScanOptionsUseCase(scanner);
		var filterService = new FilterOptionSelectionService();
		var ignoreService = new IgnoreOptionsService(localization);
		Func<string, IgnoreRules> buildIgnoreRules = _ => new IgnoreRules(
			false,
			false,
			false,
			false,
			false,
			false,
			new HashSet<string>(),
			new HashSet<string>());

		return new SelectionSyncCoordinator(
			viewModel,
			scanOptions,
			filterService,
			ignoreService,
			buildIgnoreRules,
			_ => false,
			() => null);
	}

	private static StubLocalizationCatalog CreateCatalog()
	{
		var data = new Dictionary<AppLanguage, IReadOnlyDictionary<string, string>>
		{
			[AppLanguage.En] = new Dictionary<string, string>
			{
				["Settings.Ignore.BinFolders"] = "bin folders",
				["Settings.Ignore.ObjFolders"] = "obj folders",
				["Settings.Ignore.HiddenFolders"] = "Hidden folders",
				["Settings.Ignore.HiddenFiles"] = "Hidden files",
				["Settings.Ignore.DotFolders"] = "dot folders",
				["Settings.Ignore.DotFiles"] = "dot files"
			}
		};

		return new StubLocalizationCatalog(data);
	}
}
