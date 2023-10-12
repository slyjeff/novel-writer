using NovelDocs.PageControls;
using NovelDocs.Pages.GoogleDoc;
using NovelDocs.Pages.NovelDetails;
using NovelDocs.Pages.NovelEdit;
using NovelDocs.Services;
using System.Windows.Input;

namespace NovelDocs.Pages.SectionDetails {
    internal sealed class SectionDetailsController : Controller<SectionDetailsView, SectionDetailsViewModel> {
        private ManuscriptElementTreeItem _treeItem = null!; //wil be set in the initialize

        public SectionDetailsController(IDataPersister dataPersister, IGoogleDocController googleDocController) {
            googleDocController.Hide();

            ViewModel.PropertyChanged += (_, _) => {
                dataPersister.Save();
                _treeItem.OnPropertyChanged(nameof(ManuscriptElementTreeItem.Name));
            };
        }

        public void Initialize(ManuscriptElementTreeItem treeItem) {
            ViewModel.SetSection(treeItem.ManuscriptElement);
            _treeItem = treeItem;
        }
    }
}