using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ProjectTreeViewer.Avalonia.Views;

public partial class FilterBarView : UserControl
{
    public event EventHandler<KeyEventArgs>? FilterKeyDown;
    public event EventHandler<RoutedEventArgs>? FilterCloseRequested;

    public FilterBarView()
    {
        InitializeComponent();
    }

    public TextBox? FilterBoxControl => FilterBox;

    private void OnFilterKeyDown(object? sender, KeyEventArgs e) => FilterKeyDown?.Invoke(sender, e);

    private void OnFilterClose(object? sender, RoutedEventArgs e) => FilterCloseRequested?.Invoke(sender, e);
}
