using System.Windows.Controls;
using System.Windows.Input;

namespace NovelDocs.Pages.EventBoard {
    public partial class EventBoardView {
        public EventBoardView() {
            InitializeComponent();
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (sender is not Border border) {
                return;
            }

            if (border.DataContext is not EventViewModel viewModel) {
                return;
            }

            viewModel.IsSelected = !viewModel.IsSelected;
        }
    }
}
