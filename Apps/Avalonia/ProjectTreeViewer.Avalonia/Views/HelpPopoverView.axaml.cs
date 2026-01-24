using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ProjectTreeViewer.Avalonia.Views;

public partial class HelpPopoverView : UserControl
{
    public event EventHandler<RoutedEventArgs>? CloseRequested;

    public HelpPopoverView()
    {
        InitializeComponent();
    }

    private void OnClose(object? sender, RoutedEventArgs e)
        => CloseRequested?.Invoke(sender, e);
}
