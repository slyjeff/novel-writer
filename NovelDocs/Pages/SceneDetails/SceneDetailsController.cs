using System.Linq;
using System.Threading.Tasks;
using NovelDocs.PageControls;
using NovelDocs.Pages.GoogleDoc;
using NovelDocs.Pages.NovelEdit;
using NovelDocs.Services;

namespace NovelDocs.Pages.SceneDetails; 

internal sealed class SceneDetailsController : Controller<SceneDetailsView, SceneDetailsViewModel> {
    private readonly IGoogleDocController _googleDocController;
    private ManuscriptElementTreeItem _treeItem = null!; //wil be set in the initialize

    public SceneDetailsController(IDataPersister dataPersister, IGoogleDocController googleDocController) {
        _googleDocController = googleDocController;

        ViewModel.PropertyChanged += (_, e) => {
            dataPersister.Save();

            if (e.PropertyName == nameof(ViewModel.Name)) {
                _treeItem.OnPropertyChanged(nameof(ManuscriptElementTreeItem.Name));
            }
        };

        var novel = dataPersister.GetLastOpenedNovel();
        if (novel == null) {
            return;
        }

        foreach (var character in novel.Characters) {
            ViewModel.AvailableCharacters.Add(character);
        }
    }

    public async Task Initialize(ManuscriptElementTreeItem treeItem) {
        _treeItem = treeItem;

        ViewModel.SetSourceData(treeItem.ManuscriptElement);

        ViewModel.PointOfViewCharacter = ViewModel.AvailableCharacters.FirstOrDefault(x => x.Id == treeItem.ManuscriptElement.PointOfViewCharacterId) ?? ViewModel.AvailableCharacters.First();

        await _googleDocController.Show(ViewModel);
    }

    [Command]
    public void UnassignGoogleDocId() {
        ViewModel.GoogleDocId = string.Empty;
    }
}