using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ProjectTreeViewer.Avalonia.Views;

public partial class TopMenuBarView : UserControl
{
    public event EventHandler<RoutedEventArgs>? OpenFolderRequested;
    public event EventHandler<RoutedEventArgs>? RefreshRequested;
    public event EventHandler<RoutedEventArgs>? ExitRequested;
    public event EventHandler<RoutedEventArgs>? CopyFullTreeRequested;
    public event EventHandler<RoutedEventArgs>? CopySelectedTreeRequested;
    public event EventHandler<RoutedEventArgs>? CopySelectedContentRequested;
    public event EventHandler<RoutedEventArgs>? CopyTreeAndContentRequested;
    public event EventHandler<RoutedEventArgs>? ExpandAllRequested;
    public event EventHandler<RoutedEventArgs>? CollapseAllRequested;
    public event EventHandler<RoutedEventArgs>? ZoomInRequested;
    public event EventHandler<RoutedEventArgs>? ZoomOutRequested;
    public event EventHandler<RoutedEventArgs>? ZoomResetRequested;
    public event EventHandler<RoutedEventArgs>? ToggleCompactModeRequested;
    public event EventHandler<RoutedEventArgs>? ToggleSearchRequested;
    public event EventHandler<RoutedEventArgs>? ToggleSettingsRequested;
    public event EventHandler<RoutedEventArgs>? ToggleFilterRequested;
    public event EventHandler<RoutedEventArgs>? ThemeMenuClickRequested;
    public event EventHandler<RoutedEventArgs>? LanguageRuRequested;
    public event EventHandler<RoutedEventArgs>? LanguageEnRequested;
    public event EventHandler<RoutedEventArgs>? LanguageUzRequested;
    public event EventHandler<RoutedEventArgs>? LanguageTgRequested;
    public event EventHandler<RoutedEventArgs>? LanguageKkRequested;
    public event EventHandler<RoutedEventArgs>? LanguageFrRequested;
    public event EventHandler<RoutedEventArgs>? LanguageDeRequested;
    public event EventHandler<RoutedEventArgs>? LanguageItRequested;
    public event EventHandler<RoutedEventArgs>? AboutRequested;
    public event EventHandler<RoutedEventArgs>? AboutCloseRequested;
    public event EventHandler<RoutedEventArgs>? AboutOpenLinkRequested;
    public event EventHandler<RoutedEventArgs>? AboutCopyLinkRequested;
    public event EventHandler<RoutedEventArgs>? SetLightThemeRequested;
    public event EventHandler<RoutedEventArgs>? SetDarkThemeRequested;
    public event EventHandler<RoutedEventArgs>? SetTransparentModeRequested;
    public event EventHandler<RoutedEventArgs>? SetMicaModeRequested;
    public event EventHandler<RoutedEventArgs>? SetAcrylicModeRequested;

    public TopMenuBarView()
    {
        InitializeComponent();

        var popover = ThemePopover;
        if (popover is not null)
        {
            popover.SetLightThemeRequested += (_, e) => SetLightThemeRequested?.Invoke(this, e);
            popover.SetDarkThemeRequested += (_, e) => SetDarkThemeRequested?.Invoke(this, e);
            popover.SetTransparentModeRequested += (_, e) => SetTransparentModeRequested?.Invoke(this, e);
            popover.SetMicaModeRequested += (_, e) => SetMicaModeRequested?.Invoke(this, e);
            popover.SetAcrylicModeRequested += (_, e) => SetAcrylicModeRequested?.Invoke(this, e);
        }

        var helpPopover = HelpPopover;
        if (helpPopover is not null)
        {
            helpPopover.CloseRequested += (_, e) => AboutCloseRequested?.Invoke(this, e);
            helpPopover.OpenLinkRequested += (_, e) => AboutOpenLinkRequested?.Invoke(this, e);
            helpPopover.CopyLinkRequested += (_, e) => AboutCopyLinkRequested?.Invoke(this, e);
        }
    }

    public Menu? MainMenuControl => MainMenu;

    private void OnOpenFolder(object? sender, RoutedEventArgs e) => OpenFolderRequested?.Invoke(sender, e);

    private void OnRefresh(object? sender, RoutedEventArgs e) => RefreshRequested?.Invoke(sender, e);

    private void OnExit(object? sender, RoutedEventArgs e) => ExitRequested?.Invoke(sender, e);

    private void OnCopyFullTree(object? sender, RoutedEventArgs e) => CopyFullTreeRequested?.Invoke(sender, e);

    private void OnCopySelectedTree(object? sender, RoutedEventArgs e) => CopySelectedTreeRequested?.Invoke(sender, e);

    private void OnCopySelectedContent(object? sender, RoutedEventArgs e)
        => CopySelectedContentRequested?.Invoke(sender, e);

    private void OnCopyTreeAndContent(object? sender, RoutedEventArgs e)
        => CopyTreeAndContentRequested?.Invoke(sender, e);

    private void OnExpandAll(object? sender, RoutedEventArgs e) => ExpandAllRequested?.Invoke(sender, e);

    private void OnCollapseAll(object? sender, RoutedEventArgs e) => CollapseAllRequested?.Invoke(sender, e);

    private void OnZoomIn(object? sender, RoutedEventArgs e) => ZoomInRequested?.Invoke(sender, e);

    private void OnZoomOut(object? sender, RoutedEventArgs e) => ZoomOutRequested?.Invoke(sender, e);

    private void OnZoomReset(object? sender, RoutedEventArgs e) => ZoomResetRequested?.Invoke(sender, e);

    private void OnToggleCompactMode(object? sender, RoutedEventArgs e)
        => ToggleCompactModeRequested?.Invoke(sender, e);

    private void OnToggleSearch(object? sender, RoutedEventArgs e) => ToggleSearchRequested?.Invoke(sender, e);

    private void OnToggleSettings(object? sender, RoutedEventArgs e) => ToggleSettingsRequested?.Invoke(sender, e);

    private void OnToggleFilter(object? sender, RoutedEventArgs e) => ToggleFilterRequested?.Invoke(sender, e);

    private void OnThemeMenuClick(object? sender, RoutedEventArgs e)
        => ThemeMenuClickRequested?.Invoke(sender, e);

    private void OnLangRu(object? sender, RoutedEventArgs e) => LanguageRuRequested?.Invoke(sender, e);

    private void OnLangEn(object? sender, RoutedEventArgs e) => LanguageEnRequested?.Invoke(sender, e);

    private void OnLangUz(object? sender, RoutedEventArgs e) => LanguageUzRequested?.Invoke(sender, e);

    private void OnLangTg(object? sender, RoutedEventArgs e) => LanguageTgRequested?.Invoke(sender, e);

    private void OnLangKk(object? sender, RoutedEventArgs e) => LanguageKkRequested?.Invoke(sender, e);

    private void OnLangFr(object? sender, RoutedEventArgs e) => LanguageFrRequested?.Invoke(sender, e);

    private void OnLangDe(object? sender, RoutedEventArgs e) => LanguageDeRequested?.Invoke(sender, e);

    private void OnLangIt(object? sender, RoutedEventArgs e) => LanguageItRequested?.Invoke(sender, e);

    private void OnAbout(object? sender, RoutedEventArgs e) => AboutRequested?.Invoke(sender, e);
}
