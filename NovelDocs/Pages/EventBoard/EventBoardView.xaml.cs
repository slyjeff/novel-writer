using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NovelDocs.Entity;
using NovelDocs.Pages.NovelEdit;

namespace NovelDocs.Pages.EventBoard {
    public partial class EventBoardView {
        private Point _dragStartPoint;
        private object? _itemToMove;
        
        public EventBoardView() {
            InitializeComponent();
        }

        public event Action<Character, Character>? OnMoveCharacter;

        private void Border_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (sender is not Border border) {
                return;
            }

            if (border.DataContext is not SelectableViewModel viewModel) {
                return;
            }

            viewModel.IsSelected = !viewModel.IsSelected;
        }

        private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e) {
            HorizontalScrollViewer.Width = PlotBoardScrollViewer.ActualWidth - (PlotBoardScrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible ? 18 : 0);
            VerticalScrollViewer.Height = PlotBoardScrollViewer.ActualHeight - (PlotBoardScrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible ? 18 : 0);

            HorizontalScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
            VerticalScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
        }

        private void EventBoard_OnPreviewMouseMove(object sender, MouseEventArgs e) {
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

            var data = new DataObject("eventBoardItem", _itemToMove);

            var dde = DragDropEffects.Move;
            if (e.RightButton == MouseButtonState.Pressed) {
                dde = DragDropEffects.All;
            }
            DragDrop.DoDragDrop(this, data, dde);
        }

        private void Character_OnPreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if (sender is not FrameworkElement frameworkElement) {
                return;
            }

            if (frameworkElement.DataContext is not Character character) {
                return;
            }

            _itemToMove = character;
            _dragStartPoint = e.GetPosition(null);
        }

        private void Character_OnDragOver(object sender, DragEventArgs e) {
            if (_itemToMove is not Character sourceCharacter) {
                return;
            }

            if (sender is not FrameworkElement frameworkElement) {
                return;
            }

            if (frameworkElement.DataContext is not Character destinationCharacter) {
                return;
            }

            if (sourceCharacter == destinationCharacter) {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private void Character_OnDrop(object sender, DragEventArgs e) {
            try {
                if (_itemToMove is not Character sourceCharacter) {
                    return;
                }

                if (sender is not FrameworkElement frameworkElement) {
                    return;
                }

                if (frameworkElement.DataContext is not Character destinationCharacter) {
                    return;
                }

                OnMoveCharacter?.Invoke(sourceCharacter, destinationCharacter);
            } finally {
                _dragStartPoint = default;
                _itemToMove = null;
            }
        }
    }
}
