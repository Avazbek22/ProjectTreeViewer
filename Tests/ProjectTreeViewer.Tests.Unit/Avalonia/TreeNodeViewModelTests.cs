using System.Collections.Generic;
using System.Linq;
using ProjectTreeViewer.Avalonia.ViewModels;
using ProjectTreeViewer.Kernel.Contracts;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit.Avalonia;

public sealed class TreeNodeViewModelTests
{
    [Fact]
    public void Constructor_SetsDescriptorAndDisplayName()
    {
        var descriptor = CreateDescriptor("Root");

        var node = new TreeNodeViewModel(descriptor, null, null);

        Assert.Equal(descriptor, node.Descriptor);
        Assert.Equal("Root", node.DisplayName);
        Assert.Equal("Root", node.Descriptor.DisplayName);
    }

    [Fact]
    public void FullPath_ReturnsDescriptorPath()
    {
        var descriptor = CreateDescriptor("Root");
        var node = new TreeNodeViewModel(descriptor, null, null);

        Assert.Equal(descriptor.FullPath, node.FullPath);
    }

    [Fact]
    public void Parent_IsNullForRoot()
    {
        var node = new TreeNodeViewModel(CreateDescriptor("Root"), null, null);

        Assert.Null(node.Parent);
    }

    [Fact]
    public void Parent_IsSetForChild()
    {
        var root = new TreeNodeViewModel(CreateDescriptor("Root"), null, null);
        var child = new TreeNodeViewModel(CreateDescriptor("Child"), root, null);

        Assert.Equal(root, child.Parent);
    }

    [Fact]
    public void IsExpanded_DefaultsToFalse()
    {
        var node = CreateNode("Node");

        Assert.False(node.IsExpanded);
    }

    [Fact]
    public void IsSelected_DefaultsToFalse()
    {
        var node = CreateNode("Node");

        Assert.False(node.IsSelected);
    }

    [Fact]
    public void DisplayName_Changes()
    {
        var node = CreateNode("Node");

        node.DisplayName = "Updated";

        Assert.Equal("Updated", node.DisplayName);
    }

    [Fact]
    public void IsExpanded_Changes()
    {
        var node = CreateNode("Node");

        node.IsExpanded = true;

        Assert.True(node.IsExpanded);
    }

    [Fact]
    public void IsSelected_Changes()
    {
        var node = CreateNode("Node");

        node.IsSelected = true;

        Assert.True(node.IsSelected);
    }

    [Fact]
    public void SetExpandedRecursive_ExpandsAllChildren()
    {
        var root = CreateTree();

        root.SetExpandedRecursive(true);

        Assert.All(root.Flatten(), node => Assert.True(node.IsExpanded));
    }

    [Fact]
    public void SetExpandedRecursive_DoesNotChangeChildCount()
    {
        var root = CreateTree();
        var countBefore = root.Children.Count;

        root.SetExpandedRecursive(true);

        Assert.Equal(countBefore, root.Children.Count);
    }

    [Fact]
    public void Flatten_ReturnsSelfAndDescendants()
    {
        var root = CreateTree();

        var nodes = root.Flatten().ToList();

        Assert.Equal(4, nodes.Count);
        Assert.Contains(root, nodes);
    }

    [Fact]
    public void Flatten_ReturnsPreOrderTraversal()
    {
        var root = CreateTree();

        var nodes = root.Flatten().ToList();

        Assert.Equal("Root", nodes[0].DisplayName);
        Assert.Equal("Child", nodes[1].DisplayName);
        Assert.Equal("Leaf", nodes[2].DisplayName);
        Assert.Equal("Child2", nodes[3].DisplayName);
    }

    [Fact]
    public void EnsureParentsExpanded_SetsAncestors()
    {
        var root = CreateTree();
        var leaf = root.Children[0].Children[0];

        leaf.EnsureParentsExpanded();

        Assert.True(root.IsExpanded);
        Assert.True(root.Children[0].IsExpanded);
    }

    [Fact]
    public void EnsureParentsExpanded_OnRoot_DoesNotExpandRoot()
    {
        var root = CreateTree();

        root.EnsureParentsExpanded();

        Assert.False(root.IsExpanded);
    }

    [Fact]
    public void IsChecked_SetsChildrenChecked()
    {
        var root = CreateTree();

        root.IsChecked = true;

        Assert.All(root.Children, child => Assert.True(child.IsChecked));
    }

    [Fact]
    public void IsChecked_OneChildChecked_ParentStaysUnchecked()
    {
        var root = CreateTree();

        root.Children[0].IsChecked = true;

        Assert.False(root.IsChecked);
    }

    [Fact]
    public void IsChecked_LastChildChecked_SetsParentChecked()
    {
        var root = CreateTree();

        root.Children[0].IsChecked = true;
        root.Children[1].IsChecked = true;

        Assert.True(root.IsChecked);
    }

    [Fact]
    public void IsChecked_UncheckedChildKeepsParentUnchecked()
    {
        var root = CreateTree();
        root.IsChecked = true;

        root.Children[0].IsChecked = false;

        Assert.False(root.IsChecked);
    }

    [Fact]
    public void IsChecked_AllChildrenChecked_SetsParentChecked()
    {
        var root = CreateTree();
        root.IsChecked = false;

        foreach (var child in root.Children)
            child.IsChecked = true;

        Assert.True(root.IsChecked);
    }

    [Fact]
    public void IsChecked_RecursiveParentUpdate()
    {
        var root = CreateTree();
        var leaf = root.Children[0].Children[0];

        leaf.IsChecked = true;
        root.Children[1].IsChecked = true;

        Assert.True(root.Children[0].IsChecked);
        Assert.True(root.IsChecked);
    }

    [Fact]
    public void SetExpandedRecursive_CollapsesAllChildren()
    {
        var root = CreateTree();
        root.SetExpandedRecursive(true);

        root.SetExpandedRecursive(false);

        Assert.All(root.Flatten(), node => Assert.False(node.IsExpanded));
    }

    [Fact]
    public void EnsureParentsExpanded_DoesNotChangeLeafSelection()
    {
        var root = CreateTree();
        var leaf = root.Children[0].Children[0];

        leaf.EnsureParentsExpanded();

        Assert.False(leaf.IsSelected);
    }

    [Fact]
    public void IsChecked_ParentStaysUncheckedWhenNoChildren()
    {
        var node = CreateNode("Leaf");

        node.IsChecked = true;

        Assert.True(node.IsChecked);
    }

    [Fact]
    public void Flatten_ReturnsLeafOnlyWhenNoChildren()
    {
        var node = CreateNode("Leaf");

        var nodes = node.Flatten().ToList();

        Assert.Single(nodes);
        Assert.Equal(node, nodes[0]);
    }

    private static TreeNodeViewModel CreateNode(string name)
    {
        return new TreeNodeViewModel(CreateDescriptor(name), null, null);
    }

    private static TreeNodeViewModel CreateTree()
    {
        var root = new TreeNodeViewModel(CreateDescriptor("Root"), null, null);
        var child = new TreeNodeViewModel(CreateDescriptor("Child"), root, null);
        child.Children.Add(new TreeNodeViewModel(CreateDescriptor("Leaf"), child, null));
        var secondChild = new TreeNodeViewModel(CreateDescriptor("Child2"), root, null);
        root.Children.Add(child);
        root.Children.Add(secondChild);

        return root;
    }

    private static TreeNodeDescriptor CreateDescriptor(string name, params TreeNodeDescriptor[] children)
        => new(name, $"C:\\{name}", true, false, "icon", children);

}
