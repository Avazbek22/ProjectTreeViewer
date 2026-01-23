using System.Collections.Generic;
using ProjectTreeViewer.Avalonia.ViewModels;
using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Kernel.Models;
using ProjectTreeViewer.Tests.Unit.Helpers;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit.Avalonia;

public sealed class MainWindowViewModelTests
{
    private static MainWindowViewModel CreateViewModel(IReadOnlyDictionary<string, string>? strings = null)
    {
        var catalog = new StubLocalizationCatalog(new Dictionary<AppLanguage, IReadOnlyDictionary<string, string>>
        {
            [AppLanguage.En] = strings ?? new Dictionary<string, string>()
        });
        var localization = new LocalizationService(catalog, AppLanguage.En);
        return new MainWindowViewModel(localization);
    }

    [Fact]
    public void Constructor_SetsDefaults()
    {
        var viewModel = CreateViewModel();

        Assert.True(viewModel.AllExtensionsChecked);
        Assert.True(viewModel.AllRootFoldersChecked);
        Assert.True(viewModel.AllIgnoreChecked);
        Assert.True(viewModel.IsDarkTheme);
        Assert.True(viewModel.IsTransparentEnabled);
        Assert.Equal(12, viewModel.TreeFontSize);
    }

    [Fact]
    public void Title_Changes()
    {
        var viewModel = CreateViewModel();

        viewModel.Title = "New Title";

        Assert.Equal("New Title", viewModel.Title);
    }

    [Fact]
    public void IsProjectLoaded_Changes()
    {
        var viewModel = CreateViewModel();

        viewModel.IsProjectLoaded = true;

        Assert.True(viewModel.IsProjectLoaded);
    }

    [Fact]
    public void SettingsVisible_Changes()
    {
        var viewModel = CreateViewModel();

        viewModel.SettingsVisible = true;

        Assert.True(viewModel.SettingsVisible);
    }

    [Fact]
    public void SearchVisible_Changes()
    {
        var viewModel = CreateViewModel();

        viewModel.SearchVisible = true;

        Assert.True(viewModel.SearchVisible);
    }

    [Fact]
    public void SearchQuery_Changes()
    {
        var viewModel = CreateViewModel();

        viewModel.SearchQuery = "query";

        Assert.Equal("query", viewModel.SearchQuery);
    }

    [Fact]
    public void NameFilter_Changes()
    {
        var viewModel = CreateViewModel();

        viewModel.NameFilter = "filter";

        Assert.Equal("filter", viewModel.NameFilter);
    }

    [Fact]
    public void IsDarkTheme_FalseSetsIsLightThemeTrue()
    {
        var viewModel = CreateViewModel();

        viewModel.IsDarkTheme = false;

        Assert.False(viewModel.IsDarkTheme);
        Assert.True(viewModel.IsLightTheme);
    }

    [Fact]
    public void IsCompactMode_Changes()
    {
        var viewModel = CreateViewModel();

        viewModel.IsCompactMode = true;

        Assert.True(viewModel.IsCompactMode);
    }

    [Fact]
    public void FilterVisible_Changes()
    {
        var viewModel = CreateViewModel();

        viewModel.FilterVisible = true;

        Assert.True(viewModel.FilterVisible);
    }

    [Fact]
    public void IsMicaEnabled_SetTrue_DisablesOtherEffects()
    {
        var viewModel = CreateViewModel();

        viewModel.IsMicaEnabled = true;

        Assert.True(viewModel.IsMicaEnabled);
        Assert.False(viewModel.IsAcrylicEnabled);
        Assert.False(viewModel.IsTransparentEnabled);
    }

    [Fact]
    public void IsAcrylicEnabled_SetTrue_DisablesOtherEffects()
    {
        var viewModel = CreateViewModel();

        viewModel.IsAcrylicEnabled = true;

        Assert.True(viewModel.IsAcrylicEnabled);
        Assert.False(viewModel.IsMicaEnabled);
        Assert.False(viewModel.IsTransparentEnabled);
    }

    [Fact]
    public void IsTransparentEnabled_SetTrue_DisablesOtherEffects()
    {
        var viewModel = CreateViewModel();

        viewModel.IsTransparentEnabled = true;

        Assert.True(viewModel.IsTransparentEnabled);
        Assert.False(viewModel.IsMicaEnabled);
        Assert.False(viewModel.IsAcrylicEnabled);
    }

    [Fact]
    public void ToggleTransparent_EnablesTransparentDisablesOthers()
    {
        var viewModel = CreateViewModel();
        viewModel.IsMicaEnabled = true;

        viewModel.ToggleTransparent();

        Assert.True(viewModel.IsTransparentEnabled);
        Assert.False(viewModel.IsMicaEnabled);
        Assert.False(viewModel.IsAcrylicEnabled);
    }

    [Fact]
    public void ToggleTransparent_WhenEnabled_DisablesAllEffects()
    {
        var viewModel = CreateViewModel();
        viewModel.IsTransparentEnabled = true;

        viewModel.ToggleTransparent();

        Assert.False(viewModel.IsTransparentEnabled);
        Assert.False(viewModel.HasAnyEffect);
    }

    [Fact]
    public void ToggleMica_EnablesMicaDisablesOthers()
    {
        var viewModel = CreateViewModel();
        viewModel.IsTransparentEnabled = true;

        viewModel.ToggleMica();

        Assert.True(viewModel.IsMicaEnabled);
        Assert.False(viewModel.IsTransparentEnabled);
        Assert.False(viewModel.IsAcrylicEnabled);
    }

    [Fact]
    public void ToggleMica_WhenEnabled_DisablesAllEffects()
    {
        var viewModel = CreateViewModel();
        viewModel.IsMicaEnabled = true;

        viewModel.ToggleMica();

        Assert.False(viewModel.HasAnyEffect);
    }

    [Fact]
    public void ToggleAcrylic_EnablesAcrylicDisablesOthers()
    {
        var viewModel = CreateViewModel();
        viewModel.IsTransparentEnabled = true;

        viewModel.ToggleAcrylic();

        Assert.True(viewModel.IsAcrylicEnabled);
        Assert.False(viewModel.IsTransparentEnabled);
        Assert.False(viewModel.IsMicaEnabled);
    }

    [Fact]
    public void ToggleAcrylic_WhenEnabled_DisablesAllEffects()
    {
        var viewModel = CreateViewModel();
        viewModel.IsAcrylicEnabled = true;

        viewModel.ToggleAcrylic();

        Assert.False(viewModel.HasAnyEffect);
    }

    [Fact]
    public void HasAnyEffect_TrueWhenAnyEffectEnabled()
    {
        var viewModel = CreateViewModel();
        viewModel.IsTransparentEnabled = false;

        viewModel.IsMicaEnabled = true;

        Assert.True(viewModel.HasAnyEffect);
    }

    [Fact]
    public void ShowTransparencySliders_TrueWhenAnyEffectEnabled()
    {
        var viewModel = CreateViewModel();
        viewModel.IsTransparentEnabled = false;

        viewModel.IsAcrylicEnabled = true;

        Assert.True(viewModel.ShowTransparencySliders);
    }

    [Fact]
    public void ShowTransparencySliders_FalseWhenNoEffects()
    {
        var viewModel = CreateViewModel();
        viewModel.IsTransparentEnabled = false;

        viewModel.IsAcrylicEnabled = false;
        viewModel.IsMicaEnabled = false;

        Assert.False(viewModel.ShowTransparencySliders);
    }

    [Fact]
    public void ShowBlurSlider_TrueOnlyWhenTransparentEnabled()
    {
        var viewModel = CreateViewModel();
        viewModel.IsTransparentEnabled = false;
        viewModel.IsMicaEnabled = true;

        Assert.False(viewModel.ShowBlurSlider);

        viewModel.IsTransparentEnabled = true;

        Assert.True(viewModel.ShowBlurSlider);
    }

    [Fact]
    public void ThemePopoverOpen_Changes()
    {
        var viewModel = CreateViewModel();

        viewModel.ThemePopoverOpen = true;

        Assert.True(viewModel.ThemePopoverOpen);
    }

    [Fact]
    public void MaterialIntensity_ChangesBeyondThreshold()
    {
        var viewModel = CreateViewModel();

        viewModel.MaterialIntensity = 80;

        Assert.Equal(80, viewModel.MaterialIntensity);
    }

    [Fact]
    public void BlurRadius_ChangesBeyondThreshold()
    {
        var viewModel = CreateViewModel();

        viewModel.BlurRadius = 40;

        Assert.Equal(40, viewModel.BlurRadius);
    }

    [Fact]
    public void PanelContrast_ChangesBeyondThreshold()
    {
        var viewModel = CreateViewModel();

        viewModel.PanelContrast = 70;

        Assert.Equal(70, viewModel.PanelContrast);
    }

    [Fact]
    public void BorderStrength_ChangesBeyondThreshold()
    {
        var viewModel = CreateViewModel();

        viewModel.BorderStrength = 70;

        Assert.Equal(70, viewModel.BorderStrength);
    }

    [Fact]
    public void MenuChildIntensity_ChangesBeyondThreshold()
    {
        var viewModel = CreateViewModel();

        viewModel.MenuChildIntensity = 70;

        Assert.Equal(70, viewModel.MenuChildIntensity);
    }

    [Theory]
    [InlineData(10, 12)]
    [InlineData(12, 15)]
    [InlineData(16, 20)]
    public void TreeFontSize_UpdatesTreeIconSize(double size, double expectedIconSize)
    {
        var viewModel = CreateViewModel();

        viewModel.TreeFontSize = size;

        Assert.Equal(expectedIconSize, viewModel.TreeIconSize);
    }

    [Fact]
    public void AllExtensionsChecked_Changes()
    {
        var viewModel = CreateViewModel();

        viewModel.AllExtensionsChecked = false;

        Assert.False(viewModel.AllExtensionsChecked);
    }

    [Fact]
    public void AllRootFoldersChecked_Changes()
    {
        var viewModel = CreateViewModel();

        viewModel.AllRootFoldersChecked = false;

        Assert.False(viewModel.AllRootFoldersChecked);
    }

    [Fact]
    public void AllIgnoreChecked_Changes()
    {
        var viewModel = CreateViewModel();

        viewModel.AllIgnoreChecked = false;

        Assert.False(viewModel.AllIgnoreChecked);
    }

    [Fact]
    public void SelectedFontFamily_Changes()
    {
        var viewModel = CreateViewModel();

        viewModel.SelectedFontFamily = "Segoe UI";

        Assert.Equal("Segoe UI", viewModel.SelectedFontFamily);
    }

    [Fact]
    public void PendingFontFamily_Changes()
    {
        var viewModel = CreateViewModel();

        viewModel.PendingFontFamily = "Consolas";

        Assert.Equal("Consolas", viewModel.PendingFontFamily);
    }

    [Fact]
    public void EffectToggle_SwitchesShowBlurSlider()
    {
        var viewModel = CreateViewModel();

        viewModel.ToggleMica();

        Assert.False(viewModel.ShowBlurSlider);

        viewModel.ToggleTransparent();

        Assert.True(viewModel.ShowBlurSlider);
    }

    [Fact]
    public void IsTransparentEnabled_WhenDisabled_HasAnyEffectFalse()
    {
        var viewModel = CreateViewModel();

        viewModel.IsTransparentEnabled = false;

        Assert.False(viewModel.HasAnyEffect);
    }

    [Fact]
    public void IsMicaEnabled_WhenDisabled_HasAnyEffectFalse()
    {
        var viewModel = CreateViewModel();
        viewModel.IsTransparentEnabled = false;

        viewModel.IsMicaEnabled = false;

        Assert.False(viewModel.HasAnyEffect);
    }

    [Fact]
    public void IsAcrylicEnabled_WhenDisabled_HasAnyEffectFalse()
    {
        var viewModel = CreateViewModel();
        viewModel.IsTransparentEnabled = false;

        viewModel.IsAcrylicEnabled = false;

        Assert.False(viewModel.HasAnyEffect);
    }

    [Fact]
    public void TreeIconSize_MinimumIs12()
    {
        var viewModel = CreateViewModel();

        viewModel.TreeFontSize = 1;

        Assert.Equal(12, viewModel.TreeIconSize);
    }

}
