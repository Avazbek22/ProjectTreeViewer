using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ProjectTreeViewer.Avalonia.Views;

public partial class ThemePopoverView : UserControl
{
    public event EventHandler<RoutedEventArgs>? SetLightThemeRequested;
    public event EventHandler<RoutedEventArgs>? SetDarkThemeRequested;
    public event EventHandler<RoutedEventArgs>? SetTransparentModeRequested;
    public event EventHandler<RoutedEventArgs>? SetMicaModeRequested;
    public event EventHandler<RoutedEventArgs>? SetAcrylicModeRequested;

    public ThemePopoverView()
    {
        InitializeComponent();
    }

    private void OnSetLightThemeCheckbox(object? sender, RoutedEventArgs e)
        => SetLightThemeRequested?.Invoke(sender, e);

    private void OnSetDarkThemeCheckbox(object? sender, RoutedEventArgs e)
        => SetDarkThemeRequested?.Invoke(sender, e);

    private void OnSetTransparentMode(object? sender, RoutedEventArgs e)
        => SetTransparentModeRequested?.Invoke(sender, e);

    private void OnSetMicaMode(object? sender, RoutedEventArgs e)
        => SetMicaModeRequested?.Invoke(sender, e);

    private void OnSetAcrylicMode(object? sender, RoutedEventArgs e)
        => SetAcrylicModeRequested?.Invoke(sender, e);
}
