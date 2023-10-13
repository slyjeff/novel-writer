using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using NovelDocs.PageControls;
using NovelDocs.Pages.GoogleDoc;
using NovelDocs.Pages.NovelEdit;
using NovelDocs.Services;

namespace NovelDocs.Pages.SceneDetails; 

internal sealed class SceneDetailsController : Controller<SceneDetailsView, SceneDetailsViewModel> {
    private readonly IDataPersister _dataPersister;
    private readonly IGoogleDocController _googleDocController;
    private ManuscriptElementTreeItem _treeItem = null!; //wil be set in the initialize

    public SceneDetailsController(IDataPersister dataPersister, IGoogleDocController googleDocController) {
        _dataPersister = dataPersister;
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

        foreach (var characterInScene in treeItem.ManuscriptElement.CharactersInScene.ToList()) {
            var viewModel = CreateCharacterInSceneViewModel();
            viewModel.SelectedCharacter = ViewModel.AvailableCharacters.FirstOrDefault(x => x.Id == characterInScene);
        }
    
        await _googleDocController.Show(ViewModel);
    }

    private void CharacterInSceneChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(CharacterInSceneViewModel.SelectedCharacter)) {
            return;
        }

        var charactersInScene = _treeItem.ManuscriptElement.CharactersInScene;
        charactersInScene.Clear();
        foreach (var characterInSceneViewModel in ViewModel.CharactersInScene) {
            if (!characterInSceneViewModel.IsCharacterSelected) {
                continue;
            }

            charactersInScene.Add(characterInSceneViewModel.SelectedCharacter!.Id);
        }

        _dataPersister.Save();
    }

    public void CharacterRemovedFromScene(CharacterInSceneViewModel characterInSceneViewModel) {
        characterInSceneViewModel.CharacterRemoved -= CharacterRemovedFromScene;
        characterInSceneViewModel.PropertyChanged -= CharacterInSceneChanged;
        ViewModel.CharactersInScene.Remove(characterInSceneViewModel);
    }

    private CharacterInSceneViewModel CreateCharacterInSceneViewModel() {
        var viewModel = new CharacterInSceneViewModel(ViewModel.AvailableCharacters);
        ViewModel.CharactersInScene.Add(viewModel);
        viewModel.CharacterRemoved += CharacterRemovedFromScene;
        viewModel.PropertyChanged += CharacterInSceneChanged;

        return viewModel;
    }

    [Command]
    public void UnassignGoogleDocId() {
        ViewModel.GoogleDocId = string.Empty;
    }

    [Command]
    public void AddCharacterToScene() {
        CreateCharacterInSceneViewModel();
    }
}