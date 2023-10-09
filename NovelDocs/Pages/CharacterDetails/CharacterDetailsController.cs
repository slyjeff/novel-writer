using NovelDocs.Entity;
using NovelDocs.PageControls;
using NovelDocs.Pages.NovelEdit;
using NovelDocs.Pages.SectionDetails;
using NovelDocs.Services;

namespace NovelDocs.Pages.CharacterDetails; 

internal sealed class CharacterDetailsController : Controller<CharacterDetailsView, CharacterDetailsViewModel> {
    private CharacterTreeItem _treeItem = null!; //wil be set in the initialize

    public CharacterDetailsController(IDataPersister dataPersister) {
        ViewModel.PropertyChanged += (_, _) => {
            dataPersister.Save();
            _treeItem.OnPropertyChanged(nameof(ManuscriptElementTreeItem.Name));
        };
    }

    public void Initialize(CharacterTreeItem treeItem) {
        ViewModel.SetCharacter(treeItem.Character);
        _treeItem = treeItem;
    }
}