using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NovelDocs.Pages.EventBoard {
    public partial class EventBoardView {
        public EventBoardView() {
            InitializeComponent();
        }

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
    }
}
