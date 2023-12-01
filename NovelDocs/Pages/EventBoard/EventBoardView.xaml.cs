using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NovelDocs.Entity;
using NovelDocs.PageControls;

namespace NovelDocs.Pages.EventBoard {
    public partial class EventBoardView {
        private Point _dragStartPoint;
        private object? _itemToMove;
        
        public EventBoardView() {
            InitializeComponent();
        }

        public event Action<EventViewModel>? OnEventDoubleClicked;
        public event Action<Character, Character>? OnMoveCharacter;
        public event Action<EventViewModel, EventViewModel>? OnMoveEvent;

        private void Border_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (sender is not Border border) {
                return;
            }

            if (border.DataContext is not SelectableViewModel viewModel) {
                return;
            }

            viewModel.IsSelected = !viewModel.IsSelected;
        }

        private void Border_OnLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount != 2) {
                return;
            }

            if (sender is not Border border) {
                return;
            }

            if (border.DataContext is not SelectableViewModel viewModel) {
                return;
            }

            if (viewModel is not EventViewModel eventViewModel) {
                return;
            }

            OnEventDoubleClicked?.Invoke(eventViewModel);
        }

        private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e) {
            //only accept changes from the scroll bar itself- for some reason, other controls will try to reset it back to zero when we don't want this
            if (!e.Source.Equals(PlotBoardScrollViewer)) {
                e.Handled = true;
                return;
            }

            HorizontalScrollViewer.Width = PlotBoardScrollViewer.ActualWidth - (PlotBoardScrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible ? 18 : 0);
            VerticalScrollViewer.Height = PlotBoardScrollViewer.ActualHeight - (PlotBoardScrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible ? 18 : 0);

            HorizontalScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
            VerticalScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
        }

        private void VerticalScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e) {
            PlotBoardScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
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

        private void OnPreviewMouseDown<T>(object sender, MouseButtonEventArgs e) {
            if (sender is not FrameworkElement frameworkElement) {
                return;
            }

            if (frameworkElement.DataContext is not T itemToMove) {
                return;
            }

            _itemToMove = itemToMove;
            _dragStartPoint = e.GetPosition(null);
        }

        private void Character_OnPreviewMouseDown(object sender, MouseButtonEventArgs e) {
            OnPreviewMouseDown<Character>(sender, e);
        }

        private void Event_OnPreviewMouseDown(object sender, MouseButtonEventArgs e) {
            OnPreviewMouseDown<EventViewModel>(sender, e);
        }

        private void OnDragOver<T>(object sender, DragEventArgs e) where T : class {
            if (_itemToMove is not T source) {
                return;
            }

            if (sender is not FrameworkElement frameworkElement) {
                return;
            }

            if (frameworkElement.DataContext is not T destination) {
                return;
            }

            if (source == destination) {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private void Character_OnDragOver(object sender, DragEventArgs e) {
            OnDragOver<Character>(sender, e);
        }

        private void Event_OnDragOver(object sender, DragEventArgs e) {
            OnDragOver<EventViewModel>(sender, e);
        }

        private void OnDrop<T>(object sender, Action<T, T>? action) where T : class {
            try {
                if (_itemToMove is not T source) {
                    return;
                }

                if (sender is not FrameworkElement frameworkElement) {
                    return;
                }

                if (frameworkElement.DataContext is not T destination) {
                    return;
                }

                action?.Invoke(source, destination);
            } finally {
                _dragStartPoint = default;
                _itemToMove = null;
            }
        }

        private void Character_OnDrop(object sender, DragEventArgs e) {
            OnDrop(sender, OnMoveCharacter);
        }

        private void Event_OnDrop(object sender, DragEventArgs e) {
            OnDrop(sender, OnMoveEvent);
        }
    }
}
