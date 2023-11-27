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
    }
}
