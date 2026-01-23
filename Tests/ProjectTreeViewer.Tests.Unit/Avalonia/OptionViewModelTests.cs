using System.Collections.Generic;
using ProjectTreeViewer.Avalonia.ViewModels;
using ProjectTreeViewer.Kernel.Models;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit.Avalonia;

public sealed class OptionViewModelTests
{
    [Fact]
    public void SelectionOptionViewModel_Constructor_SetsProperties()
    {
        var option = new SelectionOptionViewModel("Option", true);

        Assert.Equal("Option", option.Name);
        Assert.True(option.IsChecked);
    }

    [Fact]
    public void SelectionOptionViewModel_IsChecked_Changes()
    {
        var option = new SelectionOptionViewModel("Option", false);

        option.IsChecked = true;

        Assert.True(option.IsChecked);
    }

    [Fact]
    public void SelectionOptionViewModel_IsChecked_RaisesCheckedChanged()
    {
        var option = new SelectionOptionViewModel("Option", false);
        var called = false;
        option.CheckedChanged += (_, _) => called = true;

        option.IsChecked = true;

        Assert.True(called);
    }

    [Fact]
    public void SelectionOptionViewModel_IsChecked_SameValueDoesNotRaiseCheckedChanged()
    {
        var option = new SelectionOptionViewModel("Option", false);
        var called = false;
        option.CheckedChanged += (_, _) => called = true;

        option.IsChecked = false;

        Assert.False(called);
    }

    [Fact]
    public void IgnoreOptionViewModel_Constructor_SetsProperties()
    {
        var option = new IgnoreOptionViewModel(IgnoreOptionId.IdeGenerated, "IDE", true);

        Assert.Equal(IgnoreOptionId.IdeGenerated, option.Id);
        Assert.Equal("IDE", option.Label);
        Assert.True(option.IsChecked);
    }

    [Fact]
    public void IgnoreOptionViewModel_Label_Changes()
    {
        var option = new IgnoreOptionViewModel(IgnoreOptionId.Binary, "bin", false);

        option.Label = "binary";

        Assert.Equal("binary", option.Label);
    }

    [Fact]
    public void IgnoreOptionViewModel_IsChecked_Changes()
    {
        var option = new IgnoreOptionViewModel(IgnoreOptionId.Binary, "bin", false);

        option.IsChecked = true;

        Assert.True(option.IsChecked);
    }

    [Fact]
    public void IgnoreOptionViewModel_IsChecked_RaisesCheckedChanged()
    {
        var option = new IgnoreOptionViewModel(IgnoreOptionId.Binary, "bin", false);
        var called = false;
        option.CheckedChanged += (_, _) => called = true;

        option.IsChecked = true;

        Assert.True(called);
    }

    [Fact]
    public void IgnoreOptionViewModel_IsChecked_SameValueDoesNotRaiseCheckedChanged()
    {
        var option = new IgnoreOptionViewModel(IgnoreOptionId.Binary, "bin", false);
        var called = false;
        option.CheckedChanged += (_, _) => called = true;

        option.IsChecked = false;

        Assert.False(called);
    }

}
