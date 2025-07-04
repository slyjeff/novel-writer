using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NovelDocs.Entity;

namespace NovelDocs.Pages.NovelEdit;

public enum MoveType { Into, Before }

public partial class NovelEditView {
    public NovelEditView() {
        InitializeComponent();
    }

    public event Action<NovelTreeItem, MoveType, NovelTreeItem>? OnMoveNovelTreeItem;

    private Point _dragStartPoint;
    private TreeViewItem? _itemToMove;

    private ManuscriptElementTreeItem? _manuscriptElementToMove;
    private ManuscriptElementTreeItem? _manuscriptElementDestination;

    private void MoveInside_Click(object sender, RoutedEventArgs e) {
        if (_manuscriptElementToMove == null || _manuscriptElementDestination == null) {
            return;
        }

        OnMoveNovelTreeItem?.Invoke(_manuscriptElementToMove, MoveType.Into, _manuscriptElementDestination);
    }

    private void MoveAfter_Click(object sender, RoutedEventArgs e) {
        if (_manuscriptElementToMove == null || _manuscriptElementDestination == null) {
            return;
        }

        OnMoveNovelTreeItem?.Invoke(_manuscriptElementToMove, MoveType.Before, _manuscriptElementDestination);
    }

    private void TreeView_PreviewMouseLeftButtonDown(object sender, MouseEventArgs e) {
        var treeViewItem = FindAncestor<TreeViewItem>(sender as DependencyObject);

        if (treeViewItem?.DataContext is not (ManuscriptElementTreeItem or CharacterTreeItem)) {
            return;
        }

        _itemToMove = treeViewItem;
        _dragStartPoint = e.GetPosition(null);
    }

    private void TreeView_PreviewMouseMove(object sender, MouseEventArgs e) {
        if (_itemToMove == null) {
            return;
        }

        if (e.LeftButton != MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed) {
            _dragStartPoint = default;
            _itemToMove = null;
            return;
        }

        var position = e.GetPosition(null);
        if (Math.Abs(position.X - _dragStartPoint.X) <= SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(position.Y - _dragStartPoint.Y) <= SystemParameters.MinimumVerticalDragDistance) {
            return;
        }

        var data = new DataObject("treeViewItem", _itemToMove);

        var dde = DragDropEffects.Move;
        if (e.RightButton == MouseButtonState.Pressed) {
            dde = DragDropEffects.All;
        }
        DragDrop.DoDragDrop(TreeView, data, dde);
    }

    private void TreeView_DragOver(object sender, DragEventArgs e) {
        var treeViewItem = FindAncestor<TreeViewItem>(sender as DependencyObject);
        if (treeViewItem == null) {
            return;
        }

        if (treeViewItem == _itemToMove) {
            return;
        }

        if (_itemToMove?.DataContext is not NovelTreeItem itemToMove) {
            return;
        }

        switch (treeViewItem.DataContext) {
            case ManuscriptTreeItem when itemToMove is ManuscriptElementTreeItem:
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
                return;
            case ManuscriptElementTreeItem when itemToMove is ManuscriptElementTreeItem:
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
                return;
            case CharacterTreeItem when itemToMove is CharacterTreeItem:
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
                return;
        }

        e.Effects = DragDropEffects.None;
        e.Handled = true;
    }

    private void TreeView_Drop(object sender, DragEventArgs e) {
        try {
            var treeViewItem = FindAncestor<TreeViewItem>(sender as DependencyObject);
            if (treeViewItem == _itemToMove) {
                return;
            }

            if (treeViewItem?.DataContext is not NovelTreeItem destinationItem) {
                return;
            }

            if (_itemToMove?.DataContext is not NovelTreeItem itemToMove) {
                return;
            }

            switch (destinationItem) {
                case ManuscriptTreeItem when itemToMove is not ManuscriptElementTreeItem:
                    return;
                case ManuscriptTreeItem:
                    OnMoveNovelTreeItem?.Invoke(itemToMove, MoveType.Into, destinationItem);
                    return;
                case ManuscriptElementTreeItem when itemToMove is not ManuscriptElementTreeItem:
                    return;
                case ManuscriptElementTreeItem manuscriptElementDestinationItem when ItemToMoveAndDestinationManuscriptElementShareParentSection(itemToMove, manuscriptElementDestinationItem):
                    _manuscriptElementToMove = itemToMove as ManuscriptElementTreeItem;
                    _manuscriptElementDestination = manuscriptElementDestinationItem;
                    DragAndDropMenu.IsOpen = true;
                    return;
                case ManuscriptElementTreeItem { ManuscriptElement.Type: ManuscriptElementType.Section }:
                    if (((ManuscriptElementTreeItem)itemToMove).Parent == destinationItem) {
                        OnMoveNovelTreeItem?.Invoke(itemToMove, MoveType.Before, destinationItem);
                        return;
                    }
                    OnMoveNovelTreeItem?.Invoke(itemToMove, MoveType.Into, destinationItem);
                    return;
                case ManuscriptElementTreeItem:
                    OnMoveNovelTreeItem?.Invoke(itemToMove, MoveType.Before, destinationItem);
                    return;
                case CharacterTreeItem:
                    OnMoveNovelTreeItem?.Invoke(itemToMove, MoveType.Before, destinationItem);
                    return;
            }
        } finally {
            _dragStartPoint = default;
            _itemToMove = null;
        }
    }

    private bool ItemToMoveAndDestinationManuscriptElementShareParentSection(NovelTreeItem itemToMove, NovelTreeItem destination) {
        if (destination is not ManuscriptElementTreeItem manuscriptElementTreeDestination) {
            return false;
        }

        if (manuscriptElementTreeDestination.ManuscriptElement.Type != ManuscriptElementType.Section) {
            return false;
        }
        
        if (itemToMove is not ManuscriptElementTreeItem manuscriptElementTreeItemToMove) {
            return false;
        }

        return manuscriptElementTreeItemToMove.Parent == manuscriptElementTreeDestination.Parent;
    }

    private static T? FindAncestor<T>(DependencyObject? child) where T : DependencyObject {
        if (child == null) {
            return null;
        }

        if (child is T dependencyObject) {
            return dependencyObject;
        }

        return FindAncestor<T>(VisualTreeHelper.GetParent(child));
    }
}