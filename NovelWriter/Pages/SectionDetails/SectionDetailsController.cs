using NovelWriter.PageControls;
using NovelWriter.Pages.NovelEdit;
using NovelWriter.Services;

namespace NovelWriter.Pages.SectionDetails {
    internal sealed class SectionDetailsController : Controller<SectionDetailsView, SectionDetailsViewModel> {
        private ManuscriptElementTreeItem _treeItem = null!; //wil be set in the initialize

        public SectionDetailsController(IDataPersister dataPersister) {
            ViewModel.PropertyChanged += async (_, _) => {
                await dataPersister.Save();
                _treeItem.OnPropertyChanged(nameof(ManuscriptElementTreeItem.Name));
            };
        }

        public void Initialize(ManuscriptElementTreeItem treeItem) {
            ViewModel.SetSection(treeItem.ManuscriptElement);
            _treeItem = treeItem;
        }
    }
}