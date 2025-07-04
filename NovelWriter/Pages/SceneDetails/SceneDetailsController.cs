using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using NovelWriter.PageControls;
using NovelWriter.Pages.NovelEdit;
using NovelWriter.Pages.RichTextEditor;
using NovelWriter.Services;

namespace NovelWriter.Pages.SceneDetails; 

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class SceneDetailsController : Controller<SceneDetailsView, SceneDetailsViewModel> {
    private readonly IDataPersister _dataPersister;
    private readonly IRichTextEditorController _richTextEditorController;
    private ManuscriptElementTreeItem _treeItem = null!; //will be set in the initializer

    public SceneDetailsController(IDataPersister dataPersister, IRichTextEditorController richTextEditorController) {
        _dataPersister = dataPersister;
        _richTextEditorController = richTextEditorController;

        ViewModel.PropertyChanged += async (_, e) => {
            await dataPersister.Save();

            if (e.PropertyName == nameof(ViewModel.Name)) {
                _treeItem.OnPropertyChanged(nameof(ManuscriptElementTreeItem.Name));
            }
        };

        var novel = dataPersister.CurrentNovel;

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
    
        await _richTextEditorController.Show(ViewModel);
    }

    private async void CharacterInSceneChanged(object? sender, PropertyChangedEventArgs e) {
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

        await _dataPersister.Save();
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
    public void AddCharacterToScene() {
        CreateCharacterInSceneViewModel();
    }
}