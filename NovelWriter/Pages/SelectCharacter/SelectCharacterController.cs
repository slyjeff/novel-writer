using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NovelWriter.PageControls;
using NovelWriter.Pages.SceneDetails;
using NovelWriter.Services;

namespace NovelWriter.Pages.SelectCharacter; 

internal sealed class SelectCharacterController : Controller<SelectCharacterView, SelectCharacterViewModel> {
    private readonly IDataPersister _dataPersister;

    public SelectCharacterController(IDataPersister dataPersister) {
        _dataPersister = dataPersister;
        View.Owner = Application.Current.MainWindow;
    }

    public async Task Initialize() {
        var novel = _dataPersister.CurrentNovel;
        
        var availableCharacters = new List<CharacterWithImage>();
        foreach (var character in novel.Characters) {
            var image = await _dataPersister.GetImage(character, 50);
            availableCharacters.Add(new CharacterWithImage(character, image));
        }
        
        ViewModel.AvailableCharacters = availableCharacters;
        ViewModel.SelectedCharacter = availableCharacters.FirstOrDefault();
    }
    
    
    [Command]
    public void Ok() {
        View.DialogResult = true;
    }
}