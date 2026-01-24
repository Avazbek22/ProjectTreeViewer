using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace ProjectTreeViewer.Avalonia.Services;

public static class MessageDialog
{
    public static async Task ShowAsync(Window owner, string title, string message)
    {
        var themeVariant = owner?.ActualThemeVariant
            ?? global::Avalonia.Application.Current?.ActualThemeVariant
            ?? ThemeVariant.Default;
        var appBackground = global::Avalonia.Application.Current?.TryFindResource("AppBackgroundBrush", themeVariant, out var resource) == true
            ? resource as IBrush
            : null;

        var dialog = new Window
        {
            Title = title,
            Width = 420,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            RequestedThemeVariant = themeVariant,
            TransparencyLevelHint = new[] { WindowTransparencyLevel.None },
            Background = appBackground,
            Content = BuildContent(message)
        };

        if (owner is not null)
            await dialog.ShowDialog(owner);
        else
            dialog.Show();
    }

    private static Control BuildContent(string message)
    {
        var text = new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(12),
            VerticalAlignment = VerticalAlignment.Center
        };

        var button = new Button
        {
            Content = "OK",
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(12),
            Width = 80
        };

        var panel = new DockPanel();
        DockPanel.SetDock(button, Dock.Bottom);

        panel.Children.Add(button);
        panel.Children.Add(text);

        button.Click += (_, _) =>
            (panel.GetVisualRoot() as Window)?.Close();

        return panel;
    }
}
