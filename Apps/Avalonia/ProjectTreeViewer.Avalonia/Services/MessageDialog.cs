using Avalonia.Controls;
using Avalonia.Layout;

namespace ProjectTreeViewer.Avalonia.Services;

public static class MessageDialog
{
    public static async Task ShowAsync(Window owner, string title, string message)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 420,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
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
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(12),
            VerticalAlignment = VerticalAlignment.Center
        };

        var button = new Button
        {
            Content = "OK",
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Avalonia.Thickness(12),
            Width = 80
        };

        var panel = new DockPanel();
        DockPanel.SetDock(button, Dock.Bottom);
        panel.Children.Add(button);
        panel.Children.Add(text);

        button.Click += (_, _) => ((Window)panel.GetVisualRoot()!).Close();

        return panel;
    }
}
